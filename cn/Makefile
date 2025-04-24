export LANGUAGES := go

.DEFAULT_GOAL := build

TAG := $(shell date -u '+%Y-%m-%dT%H-%M-%SZ')

.PHONY: build
build:
	go build -ldflags "-X config.Tag=$(TAG)" -o ./bin/ .

.PHONY: windows
windows:
	GOOS=windows GOARCH=amd64 go build -ldflags "-X config.Tag=$(TAG)"" -o ./bin/ .

.PHONY: linux
linux:
	GOOS=linux GOARCH=amd64 go build -ldflags "-X config.Tag=$(TAG)"" -o ./bin/ .

.PHONY: test
test:
	go test ./...

.PHONY: run
run:
	go run -ldflags "-X config.Tag=$(TAG)"" .

.PHONY: docker
docker:
	-docker rmi -f go-app:run
	docker build -t go-app:run --build-arg TAG=$(TAG) .

.PHONY: push
push: docker
	az account set --subscription b47beaaf-7461-4b34-844a-7105d6b8c0d7
	az acr login --name torusdev
	docker tag go-app:run torusdev.azurecr.io/certnoti:$(TAG)
	docker push torusdev.azurecr.io/certnoti:$(TAG)

.PHONY: dockerrun
dockerrun: docker
	-docker rm -f go-app-srv
	docker run -p 8080:8000 --name go-app-srv go-app:run

.PHONY: dockerdbg
dockerdbg:
	-docker rm -f go-app-debug-server
	-docker rmi -f go-app:debug
	docker build -t go-app:debug --file Dockerfile_debug --build-arg TAG=$(TAG) .
	docker run -p 8000:8000 -p 4000:4000 --name go-app-debug-server go-app:debug

.PHONY: pushdbg
pushdbg:
	az account set --subscription b47beaaf-7461-4b34-844a-7105d6b8c0d7
	az acr login --name torusdev1
	docker tag go-app:debug torusdev1.azurecr.io/certnoti:$(TAG)
	docker push torusdev1.azurecr.io/certnoti:$(TAG)



# .DEFAULT_GOAL := all

# .PHONY: login
# login:
# 	az login
# 	az account set --subscription "Visual Studio Enterprise Subscription"

# .PHONY: publisht
# publisht: linux
# 	cp host.linux.json host.json
# 	func azure functionapp publish http-my-testapp2

# .PHONY: clean
# clean:
# 	rm -f go.sum
# 	rm -f bin/*

# .PHONY: dependencies
# dependencies:
# 	go mod tidy
# 	go mod vendor

# .PHONY: format
# format: dependencies
# 	golangci-lint run --fix

# .PHONY: utest
# utest: dependencies
# 	go test -v ./...

# .PHONY: build
# build: dependencies
# 	go build -o bin/

# .PHONY: all
# all: clean format utest build
