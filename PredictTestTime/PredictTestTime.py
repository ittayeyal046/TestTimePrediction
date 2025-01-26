# %%
import os;
import sys
import json
import pandas as pd
from xgboost import XGBRegressor

#############################
#Functions

# set Dummy fields
def setDummyField(df, prefixColumnText, suffixCoulmnText):
    any_column_that_suits = any(col.startswith(prefixColumnText) and
                                col.endswith(suffixCoulmnText) for col in df.columns)
    if(any_column_that_suits == False):
         #raise ValueError(f"Suffix {suffixCoulmnText} on columns {prefixColumnText} is not found")
         print(f"Suffix {suffixCoulmnText} on columns {prefixColumnText} is not found")

    # reset all relevant columns
    for column in df.columns:
        if column.startswith(prefixColumnText):
            df[column] = False

    # set relevant column
    for column in df.columns:
        if column.endswith(suffixCoulmnText):
            df[column] = True
            break
###############################
            
# %%

#%%
file_name = 'ITuffProcessedData'
file_columns_ext = '.columns'
trainingDataPath = os.path.join(os.getenv('LOCALAPPDATA'), "TTP\\TrainingData\\")

# %%
# read trained model parameters
model = XGBRegressor()
model.load_model(rf"{trainingDataPath}{file_name}.model.json")

# %%
# Read column names from file
columnsFileName = os.path.join(trainingDataPath, file_name + '.columns')
with open(columnsFileName, 'r') as file:
    column_names = [line.strip() for line in file]

#%%
# Create an empty DataFrame with the column names
df = pd.DataFrame(columns=column_names)
df = df._append(pd.Series(False, index=df.columns), ignore_index=True)

#%%
# received from args
if(len(sys.argv) != 2):
    raise ValueError("Must have 2 argument of dictionary of the variables and has " + str(len(sys.argv)) + " of: " + "[1]:" + sys.argv[1])

parameters = sys.argv[1]
parametersAsDictionary = json.loads(parameters)

IsConcurrent = parametersAsDictionary['IsConcurrent']
Patterns_Count = parametersAsDictionary['Patterns_Count']
Tests_Count = parametersAsDictionary['Tests_Count']
Mtt_Count = parametersAsDictionary['Mtt_Count']
ConcurrentFlows_Count = parametersAsDictionary['ConcurrentFlows_Count']
Shmoo_tests_count = parametersAsDictionary['Shmoo_tests_count']
PartType = parametersAsDictionary['PartType']
ProcessStep = parametersAsDictionary['ProcessStep']
ExperimentType = parametersAsDictionary['ExperimentType']

#%% 
# fill values
df['IsConcurrent'] = bool(IsConcurrent)
df['Patterns_Count'] = int(Patterns_Count)
df['Tests_Count'] = int(Tests_Count)
df['Mtt_Count'] = int(Mtt_Count)
df['ConcurrentFlows_Count'] = int(ConcurrentFlows_Count)
df['Shmoo_tests_count'] =int(Shmoo_tests_count)

#%%
setDummyField(df, 'ITuff_PartType', PartType)

setDummyField(df, 'ITuff_ProcessStep', ProcessStep)

setDummyField(df, 'ITuff_ExperimentType', ExperimentType)

# %%
predictedTestTime = model.predict(df)
print(f"output: {predictedTestTime[0]}")
