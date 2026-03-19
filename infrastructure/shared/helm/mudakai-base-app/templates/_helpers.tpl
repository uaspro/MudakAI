{{/*
Expand the name of the chart.
*/}}
{{- define "mudakai-base-app.name" -}}
{{- default .Chart.Name .Values.nameOverride | trunc 63 | trimSuffix "-" }}
{{- end }}

{{/*
Create a default fully qualified app name.
*/}}
{{- define "mudakai-base-app.fullname" -}}
{{- if .Values.fullnameOverride }}
{{- .Values.fullnameOverride | trunc 63 | trimSuffix "-" }}
{{- else }}
{{- $name := default .Chart.Name .Values.nameOverride }}
{{- if contains $name .Release.Name }}
{{- .Release.Name | trunc 63 | trimSuffix "-" }}
{{- else }}
{{- printf "%s-%s" .Release.Name $name | trunc 63 | trimSuffix "-" }}
{{- end }}
{{- end }}
{{- end }}

{{/*
Create chart name and version as used by the chart label.
*/}}
{{- define "mudakai-base-app.chart" -}}
{{- printf "%s-%s" .Chart.Name .Chart.Version | replace "+" "_" | trunc 63 | trimSuffix "-" }}
{{- end }}

{{/*
Common labels
*/}}
{{- define "mudakai-base-app.labels" -}}
helm.sh/chart: {{ include "mudakai-base-app.chart" . }}
{{ include "mudakai-base-app.selectorLabels" . }}
{{- if .Chart.AppVersion }}
app.kubernetes.io/version: {{ .Chart.AppVersion | quote }}
{{- end }}
app.kubernetes.io/managed-by: {{ .Release.Service }}
{{- end }}

{{/*
Selector labels
*/}}
{{- define "mudakai-base-app.selectorLabels" -}}
app.kubernetes.io/name: {{ include "mudakai-base-app.name" . }}
app.kubernetes.io/instance: {{ .Release.Name }}
{{- end }}

{{/*
Create the name of the service account to use
*/}}
{{- define "mudakai-base-app.serviceAccountName" -}}
{{- if .Values.serviceAccount.create }}
{{- default (include "mudakai-base-app.fullname" .) .Values.serviceAccount.name }}
{{- else }}
{{- default "default" .Values.serviceAccount.name }}
{{- end }}
{{- end }}

{{/*
Registry URL helper
*/}}
{{- define "mudakai-base-app.registryUrl" -}}
{{- if .Values.global }}
{{- .Values.global.registry | default "registry-docker-registry.container-registry.svc.cluster.local:5000" }}
{{- else }}
{{- "registry-docker-registry.container-registry.svc.cluster.local:5000" }}
{{- end }}
{{- end }}

{{/*
Full image path
*/}}
{{- define "mudakai-base-app.image" -}}
{{- $repository := .Values.image.repository }}
{{- $tag := .Values.image.tag | default .Chart.AppVersion }}
{{- if contains "/" $repository }}
{{- printf "%s:%s" $repository $tag }}
{{- else }}
{{- $registry := include "mudakai-base-app.registryUrl" . }}
{{- printf "%s/%s:%s" $registry $repository $tag }}
{{- end }}
{{- end }}
