using PagePerformanceInsights.Handler.PerformanceData.DataTypes;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace PagePerformanceInsights.SqlServerStore.Requests {
	class PreCalculatedDistributionStrategy {
		readonly string _connectionString;
		readonly IProvidePageIds _pageIdProvider;
		readonly RequestsReader _requestsReader;
		readonly IDistributionReadStrategy _backend;
		const int AllPagesPageId = 0;

		public PreCalculatedDistributionStrategy(string connectionString, IProvidePageIds pageIdProvider, RequestsReader reader, IDistributionReadStrategy backend) {
			_connectionString = connectionString;
			_pageIdProvider = pageIdProvider;
			_requestsReader= reader;
			_backend = backend;
		}

		class SpecialValues {
			public int MedianBucketIndex{get;set;}
			public int MeanBucketIndex{get;set;}
			public int _90PercentileBucketIndex{get;set;}
		}

		public Handler.PerformanceData.DataTypes.PageDurationDistributionHistogram GetPageDistribution(DateTime forDate,string forPage) {
			var buckets = GetPrecalculatedBuckets(forDate,forPage);
			
			var specialValues = GetPreCalculatedSpecialValues(forDate,forPage);
			
			
			return new PageDurationDistributionHistogram(buckets.OrderBy(b=>b.MinIncl).ToArray(),specialValues.MeanBucketIndex,specialValues.MedianBucketIndex,specialValues._90PercentileBucketIndex);
		}

		private SpecialValues GetPreCalculatedSpecialValues(DateTime forDate,string forPage) {
			
			using(var conn = new SqlConnection(_connectionString)) {
				var cmd = conn.CreateCommand();
				cmd.CommandText = "select MedianBucketIndex,MeanBucketIndex,_90PercentileBucketIndex from PreCalculatedDistributionSpecialValues where Date=@Date and PageId=@PageId";
				cmd.Parameters.Add(new SqlParameter("Date",forDate));
				cmd.Parameters.Add(new SqlParameter("PageId",GetPageId(forPage)));
				conn.Open();
				using(var rdr = cmd.ExecuteReader()) {
					while(rdr.Read()) {
						return new SpecialValues { 
							_90PercentileBucketIndex = (int)rdr["_90PercentileBucketIndex"],
							MeanBucketIndex = (int)rdr["MeanBucketIndex"], 
							MedianBucketIndex = (int)rdr["MeanBucketIndex"] 
						};
					}
					//shouldn't happen
					return new SpecialValues {};
				}
			}
		}

		private ICollection<PagePerformanceInsights.Handler.PerformanceData.DataTypes.PageDurationDistributionHistogram.Bucket> GetPrecalculatedBuckets(DateTime forDate,string forPage) {
			var buckets = new List<PagePerformanceInsights.Handler.PerformanceData.DataTypes.PageDurationDistributionHistogram.Bucket>();
			using(var conn = new SqlConnection(_connectionString)) {
				var cmd = conn.CreateCommand();
				cmd.CommandText = "select MinIncl,MaxExcl,Count,CumulativePercentage from PreCalculatedDistribution where Date=@Date and PageId=@PageID";
				cmd.Parameters.Add(new SqlParameter("Date",forDate));

				int pageIdValue = GetPageId(forPage);

				cmd.Parameters.Add(new SqlParameter("PageId",pageIdValue));

				
				conn.Open();
				using(var rdr = cmd.ExecuteReader()) {
					while(rdr.Read()) {
						
						buckets.Add(new PagePerformanceInsights.Handler.PerformanceData.DataTypes.PageDurationDistributionHistogram.Bucket {
							Count = (int)rdr["Count"],
							CumulativePercentage = (int)rdr["CumulativePercentage"],
							MinIncl = (int)rdr["MinIncl"],
							MaxExcl = (int)rdr["MaxExcl"]
						});

						//buckets[(int)rdr["BucketIndex"]] = (int)rdr["Count"];
					}

					return buckets;
				}
			}
		}

		private int GetPageId(string forPage) {
			int pageIdValue = AllPagesPageId;

			if(forPage!=null) {
				pageIdValue = _pageIdProvider.GetPageId(forPage);
			}
			return pageIdValue;
		}

		public bool HasPreCalculatedData(DateTime forDate,string forPage) {
			return HasPreCalculatedData(forDate,GetPageId(forPage));
		}

		public bool HasPreCalculatedData(DateTime forDate,int pageId) {
			using(var conn = new SqlConnection(_connectionString)) {
				var cmd = conn.CreateCommand();
				cmd.CommandText = "select count(*) from PreCalculatedDistributionSpecialValues where Date=@Date and PageId=@PageId" ;
				cmd.Parameters.Add(new SqlParameter("Date", forDate));
				cmd.Parameters.Add(new SqlParameter("PageId",pageId));
				conn.Open();
				return (int)cmd.ExecuteScalar()>0;
			}
		}

		public void PreCalculateForDate(DateTime date) {
			if(!HasPreCalculatedData(date,AllPagesPageId)) {
				PreCalculateAll(date);
			}

			var pageIds=  _requestsReader.ListAllPages(date);

			var pageNames = _pageIdProvider.GetPageNames(pageIds);

			foreach(var pageId in pageIds ) {
				if(HasPreCalculatedData(date,pageId)) {
					continue;
				}
				PreCalculateForDateAndPage(date,pageId,pageNames[pageId]);
			}
		}



		private void PreCalculateAll(DateTime date) {
			var data = _backend.GetPageDistribution(date,null);
			InsertSpecialValues(date,AllPagesPageId,GetSpecialValues(data));
			InsertBuckets(date,AllPagesPageId,data.Buckets);
		}

		private static SpecialValues GetSpecialValues(PageDurationDistributionHistogram data) {
			return new SpecialValues {
				_90PercentileBucketIndex = data._90PercentileBucketIndex,
				MedianBucketIndex = data.MedianBucketIndex,
				MeanBucketIndex = data.MeanBucketIndex
			};
		}

		private void PreCalculateForDateAndPage(DateTime date,int pageId, string pageName) {
			var data = _backend.GetPageDistribution(date,pageName);
			InsertBuckets(date,pageId,data.Buckets);
			InsertSpecialValues(date,pageId,GetSpecialValues(data));
		}

		private void InsertBuckets(DateTime date,int pageId,PageDurationDistributionHistogram.Bucket[] buckets) {
			using(var conn = new SqlConnection(_connectionString)) {
				var cmd = conn.CreateCommand();
				cmd.CommandText = @"insert into PreCalculatedDistribution (Date,PageId,MinIncl,MaxExcl,Count,CumulativePercentage)
					select @Date,@PageId,@MinIncl,@MaxExcl,@Count,@CumulativePercentage 
					where not exists (select * from PreCalculatedDistribution pcd where Date = @Date and PageId=@PageId and MinIncl=@MinIncl)";
				cmd.Parameters.Add(new SqlParameter("Date", date));
				cmd.Parameters.Add(new SqlParameter("PageId",pageId));
				cmd.Parameters.Add(new SqlParameter("MinIncl",SqlDbType.Int));
				cmd.Parameters.Add(new SqlParameter("MaxExcl",SqlDbType.Int));
				cmd.Parameters.Add(new SqlParameter("Count",SqlDbType.Int));
				cmd.Parameters.Add(new SqlParameter("CumulativePercentage",SqlDbType.Int));
				conn.Open();
				foreach(var bucket in buckets) {
					cmd.Parameters["MinIncl"].Value = bucket.MinIncl;
					cmd.Parameters["MaxExcl"].Value = bucket.MaxExcl;
					cmd.Parameters["CumulativePercentage"].Value = bucket.CumulativePercentage;
					cmd.Parameters["Count"].Value = bucket.Count;
					cmd.ExecuteNonQuery();
				}
			}
		}

		private void InsertSpecialValues(DateTime date,int pageId,SpecialValues specialValues) {
			using(var conn = new SqlConnection(_connectionString)) {
				var cmd = conn.CreateCommand();
				cmd.CommandText = @"insert into PreCalculatedDistributionSpecialValues (Date,PageId,MedianBucketIndex,MeanBucketIndex,_90PercentileBucketIndex) 
					select @Date,@PageId,@MedianBucketIndex,@MeanBucketIndex,@_90PercentileBucketIndex 
					where not exists(select * from PreCalculatedDistributionSpecialValues pcd where pcd.Date = @Date and pcd.PageId=@PageId)";
				cmd.Parameters.Add(new SqlParameter("Date", date));
				cmd.Parameters.Add(new SqlParameter("PageId",pageId));
				cmd.Parameters.Add(new SqlParameter("MedianBucketIndex",specialValues.MedianBucketIndex));
				cmd.Parameters.Add(new SqlParameter("MeanBucketIndex",specialValues.MeanBucketIndex));
				cmd.Parameters.Add(new SqlParameter("_90PercentileBucketIndex",specialValues._90PercentileBucketIndex));
				conn.Open();
				cmd.ExecuteNonQuery();
			}
		}

	
	}
}
