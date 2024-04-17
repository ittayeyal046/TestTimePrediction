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

# %%
# read input data
#parameters = json.loads(sys.argv[1])
sparkParameters = '{"ITuff_PartType_FromSpark":"H64ADNSVAL", \
"ITuff_ProcessStep_FromSpark":"CLASSHOT", \
"ITuff_ExperimentType_FromSpark":"Correlation"}'

#json_strings = parameters.split(', ')
parametersAsDictionary = json.loads(sparkParameters)

#print(parametersAsDictionary)

# %%
# read train data
file_name = 'TestPredictionResults_23-05-07_08-51-44'
file_ext = '.csv'
file_path = os.path.join(os.getenv('LOCALAPPDATA'), "TTP\\TrainingData\\")
df=pd.read_csv(os.path.join(file_path, file_name + file_ext))

# %%
# remove unneeded columns
df = df.drop(['TestProgram_Name_NA','ITuff_Temperature_NA','ITuff_SubmitterFullName_NA','ituff_EndDate_NA','ITuff_PerUnit_IsPassed_Target_NA'], axis=1)
df = df.drop(['Family'], axis=1)                               # 'Family' doesn’t change result
df = df.drop(['ITuff_BomGroup_FromSpark'], axis=1)

# %%
# combine train and input records
df = df.append(pd.Series(np.nan for _ in range(len(df.columns))), ignore_index=True)
lastLine = df.loc[df.index[-1]]

lastLine['ITuff_PartType_FromSpark'] = parametersAsDictionary['ITuff_PartType_FromSpark']
lastLine['ITuff_ProcessStep_FromSpark'] = parametersAsDictionary['ITuff_ProcessStep_FromSpark']
lastLine['ITuff_ExperimentType_FromSpark'] = parametersAsDictionary['ITuff_ExperimentType_FromSpark']
lastLine

# %%
# create dummies
df_w_dummies = pd.get_dummies(df, columns =['ITuff_PartType_FromSpark'])
df_w_dummies = pd.get_dummies(df_w_dummies, columns =['ITuff_ProcessStep_FromSpark'])
df_w_dummies = pd.get_dummies(df_w_dummies, columns =['ITuff_ExperimentType_FromSpark'])

df_w_dummies = df_w_dummies.drop(['ITuff_Lot_NA'], axis=1)

# remove train data

# %%
# load model parameters
model2 = XGBRegressor()
model2.load_model("C:\Users\ittayeya\AppData\Local\TTP\TrainingData\ITuffProcessedData_24-03-02_05-13-06.model")

# run model on input data
prediction = model2.predict(inputData)

# export output as json to c# process as std out
print(prediction)
