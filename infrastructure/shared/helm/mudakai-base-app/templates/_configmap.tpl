{{/*
ConfigMap template - renders a standard Kubernetes ConfigMap
Usage: {{ include "mudakai-base-app.configmap" . }}
*/}}
{{- define "mudakai-base-app.configmap" -}}
{{- if .Values.configMap.enabled }}
apiVersion: v1
kind: ConfigMap
metadata:
  name: {{ include "mudakai-base-app.fullname" . }}
  labels:
    {{- include "mudakai-base-app.labels" . | nindent 4 }}
data:
  {{- range $key, $value := .Values.configMap.data }}
  {{ $key }}: |
    {{- $value | nindent 4 }}
  {{- end }}
{{- end }}
{{- end }}
