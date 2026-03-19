{{/*
Deployment template - renders a standard Kubernetes Deployment
Usage: {{ include "mudakai-base-app.deployment" . }}
*/}}
{{- define "mudakai-base-app.deployment" -}}
apiVersion: apps/v1
kind: Deployment
metadata:
  name: {{ include "mudakai-base-app.fullname" . }}
  labels:
    {{- include "mudakai-base-app.labels" . | nindent 4 }}
spec:
  {{- if and .Values.autoscaling (not .Values.autoscaling.enabled) }}
  replicas: {{ .Values.replicaCount | default 1 }}
  {{- else if not .Values.autoscaling }}
  replicas: {{ .Values.replicaCount | default 1 }}
  {{- end }}
  strategy:
    type: RollingUpdate
    rollingUpdate:
      maxUnavailable: 0
      maxSurge: 1
  selector:
    matchLabels:
      {{- include "mudakai-base-app.selectorLabels" . | nindent 6 }}
  template:
    metadata:
      annotations:
        {{- if and .Values.configMap .Values.configMap.enabled }}
        checksum/config: {{ include (print $.Template.BasePath "/configmap.yaml") . | sha256sum }}
        {{- end }}
        {{- if and .Values.secret .Values.secret.enabled }}
        checksum/secret: {{ include (print $.Template.BasePath "/secret.yaml") . | sha256sum }}
        {{- end }}
        {{- if .Values.deploymentTimestamp }}
        deploymentTimestamp: {{ .Values.deploymentTimestamp | quote }}
        {{- end }}
        {{- if and .Values.dapr .Values.dapr.enabled }}
        dapr.io/enabled: "true"
        dapr.io/app-id: {{ .Values.dapr.appId | quote }}
        dapr.io/app-port: {{ .Values.dapr.appPort | quote }}
        {{- end }}
        {{- with .Values.podAnnotations }}
        {{- toYaml . | nindent 8 }}
        {{- end }}
      labels:
        {{- include "mudakai-base-app.selectorLabels" . | nindent 8 }}
        {{- with .Values.podLabels }}
        {{- toYaml . | nindent 8 }}
        {{- end }}
    spec:
      {{- if .Values.imagePullSecrets }}
      imagePullSecrets:
        {{- range .Values.imagePullSecrets }}
        - name: {{ .name }}
        {{- end }}
      {{- end }}
      {{- if and .Values.serviceAccount .Values.serviceAccount.create }}
      serviceAccountName: {{ include "mudakai-base-app.serviceAccountName" . }}
      {{- end }}
      containers:
        - name: {{ .Chart.Name }}
          image: {{ include "mudakai-base-app.image" . }}
          imagePullPolicy: {{ .Values.image.pullPolicy | default "Always" }}
          {{- $serviceEnabled := true }}
          {{- if .Values.service }}
          {{- $serviceEnabled = .Values.service.enabled | default true }}
          {{- end }}
          {{- if $serviceEnabled }}
          ports:
            - name: http
              containerPort: {{ if and .Values.service .Values.service.targetPort }}{{ .Values.service.targetPort }}{{ else }}8080{{ end }}
              protocol: TCP
          {{- end }}
          {{- if or .Values.env .Values.envFromSecret }}
          env:
            {{- with .Values.env }}
            {{- toYaml . | nindent 12 }}
            {{- end }}
            {{- range $envName, $secretRef := .Values.envFromSecret }}
            - name: {{ $envName }}
              valueFrom:
                secretKeyRef:
                  name: {{ $secretRef.secretName }}
                  key: {{ $secretRef.key }}
            {{- end }}
          {{- end }}
          {{- $configEnabled := and .Values.configMap .Values.configMap.enabled .Values.configMap.mountPath }}
          {{- $persistenceEnabled := and .Values.persistence .Values.persistence.enabled }}
          {{- if or $configEnabled $persistenceEnabled }}
          volumeMounts:
            {{- if $configEnabled }}
            - name: config
              mountPath: {{ .Values.configMap.mountPath }}
              {{- if .Values.configMap.subPath }}
              subPath: {{ .Values.configMap.subPath }}
              {{- end }}
            {{- end }}
            {{- if $persistenceEnabled }}
            - name: data
              mountPath: {{ .Values.persistence.mountPath | default "/data" }}
            {{- end }}
          {{- end }}
          {{- if and .Values.startupProbe .Values.startupProbe.enabled }}
          startupProbe:
            httpGet:
              path: {{ .Values.startupProbe.httpGet.path }}
              port: {{ .Values.startupProbe.httpGet.port | default "http" }}
            initialDelaySeconds: {{ .Values.startupProbe.initialDelaySeconds | default 5 }}
            periodSeconds: {{ .Values.startupProbe.periodSeconds | default 10 }}
            timeoutSeconds: {{ .Values.startupProbe.timeoutSeconds | default 5 }}
            failureThreshold: {{ .Values.startupProbe.failureThreshold | default 30 }}
          {{- end }}
          {{- if and .Values.livenessProbe .Values.livenessProbe.enabled }}
          livenessProbe:
            httpGet:
              path: {{ .Values.livenessProbe.httpGet.path }}
              port: {{ .Values.livenessProbe.httpGet.port | default "http" }}
            initialDelaySeconds: {{ .Values.livenessProbe.initialDelaySeconds | default 30 }}
            periodSeconds: {{ .Values.livenessProbe.periodSeconds | default 10 }}
            timeoutSeconds: {{ .Values.livenessProbe.timeoutSeconds | default 5 }}
            failureThreshold: {{ .Values.livenessProbe.failureThreshold | default 3 }}
          {{- end }}
          {{- if and .Values.readinessProbe .Values.readinessProbe.enabled }}
          readinessProbe:
            httpGet:
              path: {{ .Values.readinessProbe.httpGet.path }}
              port: {{ .Values.readinessProbe.httpGet.port | default "http" }}
            initialDelaySeconds: {{ .Values.readinessProbe.initialDelaySeconds | default 5 }}
            periodSeconds: {{ .Values.readinessProbe.periodSeconds | default 10 }}
            timeoutSeconds: {{ .Values.readinessProbe.timeoutSeconds | default 5 }}
            failureThreshold: {{ .Values.readinessProbe.failureThreshold | default 3 }}
          {{- end }}
          {{- with .Values.resources }}
          resources:
            {{- toYaml . | nindent 12 }}
          {{- end }}
      {{- if or $configEnabled $persistenceEnabled }}
      volumes:
        {{- if $configEnabled }}
        - name: config
          configMap:
            name: {{ include "mudakai-base-app.fullname" . }}
        {{- end }}
        {{- if $persistenceEnabled }}
        - name: data
          persistentVolumeClaim:
            claimName: {{ include "mudakai-base-app.fullname" . }}
        {{- end }}
      {{- end }}
      {{- with .Values.nodeSelector }}
      nodeSelector:
        {{- toYaml . | nindent 8 }}
      {{- end }}
      {{- with .Values.affinity }}
      affinity:
        {{- toYaml . | nindent 8 }}
      {{- end }}
      {{- with .Values.tolerations }}
      tolerations:
        {{- toYaml . | nindent 8 }}
      {{- end }}
{{- end }}
