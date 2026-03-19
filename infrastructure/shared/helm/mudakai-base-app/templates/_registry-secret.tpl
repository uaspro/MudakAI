{{/*
Registry Secret template - renders a Docker registry secret
Usage: {{ include "mudakai-base-app.registry-secret" . }}
*/}}
{{- define "mudakai-base-app.registry-secret" -}}
{{- if .Values.imagePullSecrets }}
{{- range .Values.imagePullSecrets }}
{{- if .create }}
apiVersion: v1
kind: Secret
metadata:
  name: {{ .name }}
  labels:
    {{- include "mudakai-base-app.labels" $ | nindent 4 }}
type: kubernetes.io/dockerconfigjson
data:
  .dockerconfigjson: {{ .dockerconfigjson }}
{{- end }}
{{- end }}
{{- end }}
{{- end }}
