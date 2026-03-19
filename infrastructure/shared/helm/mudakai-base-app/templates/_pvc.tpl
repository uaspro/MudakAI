{{/*
PVC template - renders a standard Kubernetes PersistentVolumeClaim
Usage: {{ include "mudakai-base-app.pvc" . }}
*/}}
{{- define "mudakai-base-app.pvc" -}}
{{- if .Values.persistence.enabled }}
apiVersion: v1
kind: PersistentVolumeClaim
metadata:
  name: {{ include "mudakai-base-app.fullname" . }}
  labels:
    {{- include "mudakai-base-app.labels" . | nindent 4 }}
spec:
  accessModes:
    - {{ .Values.persistence.accessMode | default "ReadWriteOnce" }}
  {{- if .Values.persistence.storageClass }}
  storageClassName: {{ .Values.persistence.storageClass }}
  {{- end }}
  resources:
    requests:
      storage: {{ .Values.persistence.size | default "1Gi" }}
{{- end }}
{{- end }}
