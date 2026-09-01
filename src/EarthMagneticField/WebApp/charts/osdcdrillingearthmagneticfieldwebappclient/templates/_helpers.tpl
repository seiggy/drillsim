{{- define "earthmagneticfieldwebapp.name" -}}{{ .Chart.Name | trunc 63 | trimSuffix "-" }}{{- end }}
{{- define "earthmagneticfieldwebapp.fullname" -}}{{ default (printf "%s-%s" .Release.Name .Chart.Name) .Values.fullnameOverride | trunc 63 | trimSuffix "-" }}{{- end }}
{{- define "earthmagneticfieldwebapp.labels" -}}
helm.sh/chart: {{ printf "%s-%s" .Chart.Name .Chart.Version | replace "+" "_" }}
app.kubernetes.io/name: {{ include "earthmagneticfieldwebapp.name" . }}
app.kubernetes.io/instance: {{ .Release.Name }}
app.kubernetes.io/version: {{ .Chart.AppVersion | quote }}
app.kubernetes.io/managed-by: {{ .Release.Service }}
{{- end }}
{{- define "earthmagneticfieldwebapp.selectorLabels" -}}
app.kubernetes.io/name: {{ include "earthmagneticfieldwebapp.name" . }}
app.kubernetes.io/instance: {{ .Release.Name }}
{{- end }}
{{- define "earthmagneticfieldwebapp.serviceAccountName" -}}{{ if .Values.serviceAccount.create }}{{ default (include "earthmagneticfieldwebapp.fullname" .) .Values.serviceAccount.name }}{{ else }}{{ default "default" .Values.serviceAccount.name }}{{ end }}{{- end }}
