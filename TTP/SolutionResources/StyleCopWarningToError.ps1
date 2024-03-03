param ($rulesetPath = ".\")
$path = Resolve-Path "$rulesetPath\StyleCop.Analyzers.ruleset";
if (Test-Path $path) {}

$xml = [xml](Get-Content $path);
$node = $xml.RuleSet.Rules
$rules = $node.Rule | where {$_.Action -eq 'Warning'};
$rules.SetAttribute("Action","Error");
$xml.Save($path);