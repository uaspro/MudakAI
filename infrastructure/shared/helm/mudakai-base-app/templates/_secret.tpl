{{/*
Secret template - renders a standard Kubernetes Secret
Usage: {{ include "mudakai-base-app.secret" . }}
*/}}
{{- define "mudakai-base-app.secret" -}}
{{- if .Values.secret.enabled }}
apiVersion: v1
kind: Secret
metadata:
  name: {{ include "mudakai-base-app.fullname" . }}
  labels:
    {{- include "mudakai-base-app.labels" . | nindent 4 }}
type: Opaque
stringData:
  {{- range $key, $value := .Values.secret.data }}
  {{ $key }}: {{ $value | quote }}
  {{- end }}
{{- end }}
{{- end }}
