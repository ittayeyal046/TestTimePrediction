# %%
import sys
import json
import pandas as pd
import pylab as py
import matplotlib.pyplot as plt
import sklearn.model_selection as mods
import sklearn.linear_model as sklin
import sklearn.tree as st
import os;
from xgboost import XGBRegressor

#%%
file_name = 'TestPredictionResults_23-05-07_08-51-44'
file_columns_ext = '.columns'
trainingDataPath = os.path.join(os.getenv('LOCALAPPDATA'), "TTP\\TrainingData\\")

# %%
# read trained model parameters
model = XGBRegressor()
model.load_model(r"C:\Users\ittayeya\AppData\Local\TTP\TrainingData\TestPredictionResults_23-05-07_08-51-44.model")

# %%
# Read column names from file
with open(os.path.join(trainingDataPath, file_name + '.columns'), 'r') as file:
    column_names = [line.strip() for line in file]

#%%
# Create an empty DataFrame with the column names
df = pd.DataFrame(columns=column_names)
df = df.append(pd.Series(False, index=df.columns), ignore_index=True)

#%%
# received from args
parameters = sys.argv[1]
parametersAsDictionary = json.loads(parameters)

IsConcurrent = parametersAsDictionary['IsConcurrent']
Patterns_Count = parametersAsDictionary['Patterns_Count ']
Tests_Count = parametersAsDictionary['Tests_Count']
Mtt_Count = parametersAsDictionary['Mtt_Count']
ConcurrentFlows_Count = parametersAsDictionary['ConcurrentFlows_Count']
Shmoo_tests_count = parametersAsDictionary['Shmoo_tests_count']
PartType = parametersAsDictionary['PartType']
ProcessStep = parametersAsDictionary['ProcessStep']
ExperimentType = parametersAsDictionary['ExperimentType']

#%% 
# fill values
df['IsConcurrent'] = IsConcurrent
df['Patterns_Count'] = Patterns_Count
df['Tests_Count'] = Tests_Count
df['Mtt_Count'] = Mtt_Count
df['ConcurrentFlows_Count'] = ConcurrentFlows_Count
df['Shmoo_tests_count'] = Shmoo_tests_count

#%%
setDummyField(df, 'ITuff_PartType_', PartType)

setDummyField(df, 'ITuff_ProcessStep_', ProcessStep)

setDummyField(df, 'ITuff_ExperimentType_', ExperimentType)

# %%
predictedTestTime = model.predict(df)
print(predictedTestTime)

#%%
#############################
#Functions

# set Dummy fields
def setDummyField(df, prefixColumnText, suffixCoulmnText):
    for column in df.columns:
        if column.startswith(prefixColumnText):
            df[column] = False

    for column in df.columns:
        if column.endswith(suffixCoulmnText):
            df[column] = True