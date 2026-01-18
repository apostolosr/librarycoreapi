.PHONY test:
	dotnet test

.PHONY build:
	dotnet build

.PHONY dev:
	docker-compose up --build

.PHONY stop:
	docker-compose down

.PHONY restart:
	docker-compose down
	docker-compose up --build

.PHONY migrate:
	docker exec -it librarycoreapi-app dotnet ef migrations add InitialCreate
	docker exec -it librarycoreapi-app dotnet ef database update

.PHONY migrate-remove:
	docker exec -it librarycoreapi-app dotnet ef migrations remove

.PHONY seed:
	docker exec -it librarycoreapi-app dotnet run --project Database/DatabaseSeeder.cs

.PHONY publish:
	docker-compose -f docker-compose.prod.yml up --build