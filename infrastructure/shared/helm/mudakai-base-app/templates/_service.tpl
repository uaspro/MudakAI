{{/*
Service template - renders a standard Kubernetes Service
Usage: {{ include "mudakai-base-app.service" . }}
*/}}
{{- define "mudakai-base-app.service" -}}
{{- if .Values.service.enabled }}
apiVersion: v1
kind: Service
metadata:
  name: {{ include "mudakai-base-app.fullname" . }}
  labels:
    {{- include "mudakai-base-app.labels" . | nindent 4 }}
  {{- with .Values.service.annotations }}
  annotations:
    {{- toYaml . | nindent 4 }}
  {{- end }}
spec:
  type: {{ .Values.service.type | default "ClusterIP" }}
  ports:
    - port: {{ .Values.service.port | default 80 }}
      targetPort: http
      protocol: TCP
      name: http
  selector:
    {{- include "mudakai-base-app.selectorLabels" . | nindent 4 }}
{{- end }}
{{- end }}
