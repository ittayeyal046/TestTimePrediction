# %%
import pandas as pd
import numpy as np
import seaborn as sb
import pylab as py

# %%
df=pd.read_csv('./TestPredictionResults_23-03-14_11-31-48.csv')

# %%
df.head()

# %%
#df = df.drop(df[df['ITuff_PerUnit_IsPassed_Target_NA'] == False].index)


# %%
#sb.pairplot(df) 

# %%
sb.displot(df['ITuff_PerUnit_testTimeInMS_Target'])

# %%
#df.corr()

# %%
df.describe()

# %%
df.groupby('IsConcurrent').count()

# %%
df.info()

# %%
df.drop(['TestProgram_Name_NA','ITuff_Temperature_NA','ITuff_SubmitterFullName_NA','ITuff_Lot_NA', 'ituff_EndDate_NA','ITuff_PerUnit_IsPassed_Target_NA'], axis=1, inplace=True)

# %%
df.info()

# %%
df = pd.get_dummies(df, columns =['Family'])
df = pd.get_dummies(df, columns =['ITuff_PartType_FromSpark'])
df = pd.get_dummies(df, columns =['ITuff_BomGroup_FromSpark'])
df = pd.get_dummies(df, columns =['ITuff_ProcessStep_FromSpark'])
df = pd.get_dummies(df, columns =['ITuff_ExperimentType_FromSpark'])


# %%
df.info()

# %%
df.describe()

# %%
#df.Family_RaptorLake.value_counts()

# %%
#sb.pairplot(df,hue='ITuff_PerUnit_testTimeInMS_Target')

# %%
x = df.drop(['ITuff_PerUnit_testTimeInMS_Target'],axis=1)
y = df.ITuff_PerUnit_testTimeInMS_Target

# %%
import sklearn.model_selection as mods

# %%
x_train, x_test, y_train, y_test = mods.train_test_split(x, y, test_size=0.20,random_state=101)

# %%
import sklearn.linear_model as sklin
import sklearn.tree as st


# %%
model = sklin.LinearRegression()

# %%
model.fit(x_train,y_train)

# %%
y_pred = model.predict(x_test)

# %%
import matplotlib.pyplot as plt

# %%
plt.scatter(y_test,y_pred)

# %%
from sklearn.metrics import mean_squared_error, r2_score, mean_absolute_error

# %%
mse = mean_squared_error(y_test, y_pred)
rmse = np.sqrt(mse)
r2 = r2_score(y_test, y_pred)
mae = mean_absolute_error(y_test, y_pred)

# print the results
print("MSE: ", mse)
print("RMSE: ", rmse)
print("R-squared: ", r2)
print("MAE: ", mae)

# %%



