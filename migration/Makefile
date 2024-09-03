format:
	python -m isort .
	python -m black .

lint:
	python -m isort . -c
	python -m black . --check
	pylint . --recursive=y

test:
	coverage run -m unittest discover -s py_acli
	coverage report
	coverage run -m unittest discover -s automations/helpers
	coverage report

all:
	make format
	make lint
	make test
