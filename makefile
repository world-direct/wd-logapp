imagename = worlddirect/wd-logapp
build: 
	docker build -t $(imagename) .

push: build
	docker push $(imagename)


k8s-namespace = default
k8s-deployment = wd-logapp
deploy:
	kubectl -n $(k8s-namespace) create deployment $(k8s-deployment) --image=$(imagename)
	