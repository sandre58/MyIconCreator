# Changelog for {{ .Info.Title }}

All notable changes to this project will be documented in this file.  
This file is generated automatically based on commit history and tags.

{{ if .Unreleased.CommitGroups }}
## [Unreleased]
{{ range .Unreleased.CommitGroups }}
### {{ .Title }}
{{ range .Commits }}
- {{ if .Scope }}**{{ .Scope }}:** {{ end }}{{ .Subject }}
{{ end }}
{{ end }}
{{ end }}

{{ range .Versions }}
## [{{ replace .Tag.Name (printf "%s/v" $.Info.Title) "" -1 }}] - {{ .Tag.Date | date "2006-01-02" }}

{{ range .CommitGroups }}
### {{ .Title }}
{{ range .Commits }}
- {{ if .Scope }}**{{ .Scope }}:** {{ end }}{{ .Subject }} *(commit by {{ with .Author }}{{ .Name }}{{ end }}{{ if .Hash }} in [{{ .Hash.Short }}]({{ $.Info.RepositoryURL }}/commit/{{ .Hash.Long }}){{ end }})*
{{ end }}
{{ end }}

{{ if .RevertCommits }}
### 🔄 Reverts
{{ range .RevertCommits }}
- {{ .Revert.Header }}
{{ end }}
{{ end }}

{{ if .MergeCommits }}
### 🔀 Merges
{{ range .MergeCommits }}
- {{ .Header }}
{{ end }}
{{ end }}

{{ if .NoteGroups }}
### ⚠️ Notes
{{ range .NoteGroups }}
- {{ .Body }}
{{ end }}
{{ end }}

{{ if .Tag.Previous }}
**Full Changelog:** [compare {{ .Tag.Previous.Name }}...{{ .Tag.Name }}]({{ $.Info.RepositoryURL }}/compare/{{ .Tag.Previous.Name }}...{{ .Tag.Name }})
{{ end }}

---
{{ end }}
