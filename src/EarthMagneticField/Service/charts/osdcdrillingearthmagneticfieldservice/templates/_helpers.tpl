{{- define "earthmagneticfieldservice.name" -}}{{ .Chart.Name | trunc 63 | trimSuffix "-" }}{{- end }}
{{- define "earthmagneticfieldservice.fullname" -}}{{ default (printf "%s-%s" .Release.Name .Chart.Name) .Values.fullnameOverride | trunc 63 | trimSuffix "-" }}{{- end }}
{{- define "earthmagneticfieldservice.labels" -}}
helm.sh/chart: {{ printf "%s-%s" .Chart.Name .Chart.Version | replace "+" "_" }}
app.kubernetes.io/name: {{ include "earthmagneticfieldservice.name" . }}
app.kubernetes.io/instance: {{ .Release.Name }}
app.kubernetes.io/version: {{ .Chart.AppVersion | quote }}
app.kubernetes.io/managed-by: {{ .Release.Service }}
{{- end }}
{{- define "earthmagneticfieldservice.selectorLabels" -}}
app.kubernetes.io/name: {{ include "earthmagneticfieldservice.name" . }}
app.kubernetes.io/instance: {{ .Release.Name }}
{{- end }}
{{- define "earthmagneticfieldservice.serviceAccountName" -}}{{ if .Values.serviceAccount.create }}{{ default (include "earthmagneticfieldservice.fullname" .) .Values.serviceAccount.name }}{{ else }}{{ default "default" .Values.serviceAccount.name }}{{ end }}{{- end }}
