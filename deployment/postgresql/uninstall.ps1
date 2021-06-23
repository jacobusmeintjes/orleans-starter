kubectl delete -n default service postgres
kubectl delete -n default deployment postgres

kubectl delete -n default PersistentVolumeClaim postgres-pv-claim
kubectl delete -n default persistentvolume postgres-pv-volume

kubectl delete -n default ConfigMap postgres-config

