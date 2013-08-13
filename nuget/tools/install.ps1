param($installPath, $toolsPath, $package, $project)

#$project.ProjectItems  | % { write-host $_.Name }

$item = $project.ProjectItems.Item("PPI").ProjectItems.Item("SqlServer").ProjectItems.Item("schema.sql")

$item.Properties.Item("BuildAction").Value = [int]0
