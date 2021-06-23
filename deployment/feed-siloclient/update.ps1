kubectl scale -n default deployment feedstarter-deployment --replicas=0


Start-Sleep 10

kubectl scale -n default deployment feedstarter-deployment --replicas=1

