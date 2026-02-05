.PHONY: verification build-back test-back build-front

verification: build-back build-front test-back

build-back:
	dotnet build Monet.sln

test-back:
	dotnet test Monet.sln

build-front:
	cd src/Presenters/Monet.WebApp/ClientApp && npm install
	cd src/Presenters/Monet.WebApp/ClientApp && npm run build
