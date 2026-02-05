{{/*
Full image name for a component
*/}}
{{- define "nfl-wallet.image" -}}
{{- $ns := .root.Values.imageNamespace -}}
{{- $reg := default "quay.io" .root.Values.global.imageRegistry -}}
{{- printf "%s/%s/%s:%s" $reg $ns .image .tag -}}
{{- end -}}

{{- define "nfl-wallet.fullname" -}}
{{- printf "%s-%s" (include "nfl-wallet.releaseName" .) .component -}}
{{- end -}}

{{- define "nfl-wallet.releaseName" -}}
{{- default .Release.Name .Values.nameOverride | trunc 63 | trimSuffix "-" -}}
{{- end -}}
