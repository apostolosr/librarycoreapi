test:
	dotnet test

build:
	dotnet build

dev:
	docker-compose up --build

stop:
	docker-compose down

clean:
	docker-compose down -v
	rm -rf Migrations/

restart:
	docker-compose down
	docker-compose up --build

migrate:
	docker exec -it librarycoreapi-app dotnet ef migrations add InitialCreate
	docker exec -it librarycoreapi-app dotnet ef database update

migrate-remove:
	docker exec -it librarycoreapi-app dotnet ef migrations remove

seed:
	docker exec -it librarycoreapi-app dotnet run -- seed
	

publish:
	docker-compose -f docker-compose.prod.yml up --build

.PHONY: test build dev stop clean restart migrate migrate-remove seed publish