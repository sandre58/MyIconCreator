{{ if .Unreleased.CommitGroups -}}
## [Unreleased]
{{ range .Unreleased.CommitGroups -}}
### {{ .Title }}
{{ range .Commits -}}
- {{ if .Scope }}**{{ .Scope }}:** {{ end }}{{ .Subject }}
{{ end }}
{{ end }}
{{ end }}
{{ range .Versions }}
 
{{ range .CommitGroups -}}
### {{ .Title }}
{{ range .Commits -}}
- {{ if .Scope }}**{{ .Scope }}:** {{ end }}{{ .Subject }} *(commit by **{{ with .Author }}{{ .Name }}**{{ end }}{{ if .Hash }} in [{{ .Hash.Short }}]({{ $.Info.RepositoryURL }}/commit/{{ .Hash.Long }}){{ end }})*
{{ end }}
{{ end }}

{{ if .RevertCommits -}}
### 🔄 Reverts
{{ range .RevertCommits -}}
- {{ .Revert.Header }}
{{ end }}
{{ end }}

{{ if .MergeCommits -}}
### 🔀 Merges
{{ range .MergeCommits -}}
- {{ .Header }}
{{ end }}
{{ end }}

{{ if .NoteGroups -}}
{{ range .NoteGroups -}}
// breaking changes or notes
### ⚠️ {{ .Title }}
{{ range .Notes }}
- {{ .Body }}
{{ end }}
{{ end }}
{{ end }}

---

{{- $basePath := "" -}}
{{- if (hasSuffix $.Info.Title ".Wpf") -}}
  {{- $basePath = printf "src/wpf/%s" $.Info.Title -}}
{{- else if (hasSuffix $.Info.Title ".Avalonia") -}}
  {{- $basePath = printf "src/avalonia/%s" $.Info.Title -}}
{{- else -}}
  {{- $basePath = printf "src/%s" $.Info.Title -}}
{{- end -}}

{{ "" }}  <!-- force a line break -->

📖 [Full Changelog]({{ $.Info.RepositoryURL }}/blob/main/{{ $basePath }}/CHANGELOG.md)
{{ end }}