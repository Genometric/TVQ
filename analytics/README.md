
Run `python .\run.py [PATH TO DATA] [{TRUE/FALSE} PLOT CITATION CHANGES] [CLUSTER COUNT]`

## Setup Environment 
```shell
$ virtualenv .venv
$ source .venv/bin/activate
$ pip install -r requirements.txt
```


## Debug and Test
To execute tests, activate virtual env and run:

```shell
$ pwd
# Ensure you are at the root folder containing of the project.

$ pytest -s
```
