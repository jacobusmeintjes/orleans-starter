#Go to project folder
cd..
cd src
cd feed-siloclient

dotnet publish . -c Release -o publish

docker rmi feedstarter:1.0.0 -f


docker build .\. -t feedstarter:1.0.0

#Move to deployment folder
cd..
cd..
cd deployment
kubectl delete -n default service feedstarter-service

kubectl delete -n default deployment feedstart-deployment

kubectl -f ./deployment.yaml apply

kubectl -f ./services.yaml apply