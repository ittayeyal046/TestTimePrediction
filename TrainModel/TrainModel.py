# %%
import pandas as pd
import numpy as np
import matplotlib as matplotlib
import matplotlib.pyplot as plt
import sklearn.model_selection as mods
import os
import matplotlib.pyplot as plt

###########################
# Functions
###########################

# if to show graphs (needed for cmd run)
def configure_graph_display(showGraphs):
    if not showGraphs:
        matplotlib.use('agg')

def load_training_data():
    file_name = 'ITuffProcessedData'
    file_csv_ext = '.csv'
    trainingDataPath = os.path.join(os.getenv('LOCALAPPDATA'), "TTP\\TrainingData\\")
    df=pd.read_csv(os.path.join(trainingDataPath, file_name + file_csv_ext))
    return file_name,file_csv_ext,trainingDataPath,df

# Graph of Tests Passed / Failed
def plot_tests_passed_failed(df):
    # data on number of tests passed and failed
    testFailureData = {
        '# Tests Passed': len(df[df['ITuff_PerUnit_IsPassed_Target_NA'] == True]), 
        '# Tests Failed': len(df[df['ITuff_PerUnit_IsPassed_Target_NA'] == False])
    }

    # Convert the dictionary to a pandas DataFrame
    df1 = pd.DataFrame.from_dict(testFailureData, orient='index', columns=['Tests'])

    # create a pie chart
    fig, ax = plt.subplots(figsize=(8, 6))
    explode = (0.1, 0)  # explode the first slice

    # calculate the percentages and display them on the chart
    percentages = df1['Tests'] / df1['Tests'].sum() * 100
    labels = [f'{df1.index[i]}: {df1.Tests.iloc[i]:,.0f} ({percentages[i]:.1f}%)' for i in range(len(df1))]
    ax.pie(df1['Tests'], explode=explode, labels=labels, autopct='', startangle=90)
    ax.axis('equal')

    # set the title of the chart
    plt.title('# Tests Passed / Failed')

    # display the chart
    plt.show()

def plot_all_tests_time_histogram(df, max_time_filter):
    # all tests time histogram
    all_tests_time = df[df['ITuff_PerUnit_testTimeInMS_Target'] < max_time_filter]
    plt.hist(all_tests_time['ITuff_PerUnit_testTimeInMS_Target'], bins=100)

    # Add labels and titles
    plt.xlabel('time in seconds')
    plt.ylabel('test amount')
    plt.title('Histogram of all tests time')

    # Show the plot
    plt.show()

def plot_predict_vs_actual(y_check):
    plt.hist(y_check, 40)
    plt.xlabel('predict vs actual % accuracy')
    plt.ylabel('tests amount')
    plt.show()

def plot_failed_tests_time_histogram(df, max_time_filter):
    # failed tests time histogram
    failed_records = df[df['ITuff_PerUnit_IsPassed_Target_NA'] == False]
    plt.hist(failed_records['ITuff_PerUnit_testTimeInMS_Target'], bins=100)

    # Add labels and titles
    plt.xlabel('time in seconds')
    plt.ylabel('test amount')
    plt.title('Histogram of failed tests time')
    plt.xlim(0, max_time_filter)

    # Show the plot
    plt.show()

def plot_test_time_categories(df):
    min_time = 50
    max_time = 500

    # add graph of how many above max_time
    total = len(df['ITuff_PerUnit_testTimeInMS_Target'])

    filtered_by_min_and_max = df[(df['ITuff_PerUnit_testTimeInMS_Target'] > max_time) | (df['ITuff_PerUnit_testTimeInMS_Target']< min_time)]
    count = len(filtered_by_min_and_max)

    categories = ['between '+str(min_time) + ' and ' +str(max_time), 'other']
    values = [total-count, count]

    # Set up the figure and axis
    fig, ax = plt.subplots()

    # Set the x-axis ticks and labels
    ax.set_xticks(range(len(categories)))
    ax.set_xticklabels(categories)

    # Create the bar plot
    ax.bar(range(len(categories)), values)

    # Add values to the bars
    for i, val in enumerate(values):
        ax.text(i, val + 0.5, str(val), ha='center', fontsize=10, color='red')

    # Show the plot
    plt.show()
    return filtered_by_min_and_max

def plot_accuracy_by_ranges(y_check, x_train_wo_LOT):
    ranges = [75, 80, 85, 90, 95, 105 ,110,115,120,125]

    # Compute the histogram
    counts, _ = np.histogram(y_check, bins=ranges)
    y_check_percentage = counts / len(y_check) * 100

    labels = [f'{ranges[i]}-{ranges[i+1]}' for i in range(len(ranges)-1)]
    plt.bar(labels, y_check_percentage)
    plt.xlabel('Accuracy between predicted/actual (%)', fontsize=8)
    plt.ylabel('Percentage of the tests (%)')
    plt.title('Accuracy of prediction')
    plt.xticks(rotation=45, ha='center')
    plt.show()

def drop_unneeded_columns(x_w_minMax, x_train, x_test):
    x_train_wo_LOT = x_train.drop(['ITuff_Lot_NA'], axis=1)
    x_test_wo_LOT = x_test.drop(['ITuff_Lot_NA'], axis=1)
    x_w_minMax_wo_LOT = x_w_minMax.drop(['ITuff_Lot_NA'], axis=1)
    return x_train_wo_LOT,x_test_wo_LOT,x_w_minMax_wo_LOT

def create_dummies(df):
    df_w_dummies = pd.get_dummies(df, columns =['ITuff_PartType_FromSpark'])
    df_w_dummies = pd.get_dummies(df_w_dummies, columns =['ITuff_ProcessStep_FromSpark'])
    df_w_dummies = pd.get_dummies(df_w_dummies, columns =['ITuff_ExperimentType_FromSpark'])

    # df_complete = pd.get_dummies(df_complete, columns =['ITuff_PartType_FromSpark'])
    # df_complete = pd.get_dummies(df_complete, columns =['ITuff_ProcessStep_FromSpark'])
    # df_complete = pd.get_dummies(df_complete, columns =['ITuff_ExperimentType_FromSpark'])

    # df_filtered = pd.get_dummies(df_filtered, columns =['ITuff_BomGroup_FromSpark'])
    # df_filtered = pd.get_dummies(df_filtered, columns =['Family'])

    df_w_dummies.describe()

    df_w_dummies.groupby('IsConcurrent').count()

    # df_w_dummies.info()
    return df_w_dummies

def plot_success_tests_time_histogram(df, max_time_filter):
    # success tests time histogram
    success_records = df[df['ITuff_PerUnit_IsPassed_Target_NA'] == True]
    plt.hist(success_records['ITuff_PerUnit_testTimeInMS_Target'], bins=100)

    # Add labels and titles
    plt.xlabel('time in seconds')
    plt.ylabel('test amount')
    plt.title('Histogram of success tests time')
    plt.xlim(0, max_time_filter)

    # Show the plot
    plt.show()

def drop_unwanted_columns(df):
    df = df.drop(['TestProgram_Name_NA','ITuff_Temperature_NA','ITuff_SubmitterFullName_NA','ituff_EndDate_NA','ITuff_PerUnit_IsPassed_Target_NA'], axis=1)
    df = df.drop(['Family'], axis=1)                               # 'Family' doesn’t change result
    df = df.drop(['ITuff_BomGroup_FromSpark'], axis=1)

    # testings for other columns removal
    #df.drop(['ConcurrentFlows_Count'], axis=1, inplace=True)                # ConcurrentFlows_Count doesn't change result
    #df.drop(['IsConcurrent'], axis=1, inplace=True)                         # 'IsConcurrent' doesn’t change result
    #df.drop(['Patterns_Count'], axis=1, inplace=True)                       # Patterns_Count helps a little
    #df.drop(['Tests_Count'], axis=1, inplace=True)                         # Tests_Count helps a little
    #df.drop(['Shmoo_tests_count'], axis=1, inplace=True)                   # Shmoo_tests_count helps a little
    #df.drop(['ITuff_ProcessStep_FromSpark'], axis=1, inplace=True)         # ITuff_ProcessStep_FromSpark is critical
    #df.drop(['ITuff_ExperimentType_FromSpark'], axis=1, inplace=True)      # ITuff_ExperimentType_FromSpark helps a little

    return df

def drop_below_min_and_above_max(filtered_by_min_and_max, df_w_dummies):
    df_w_dummies_wo_minMax = df_w_dummies.drop(filtered_by_min_and_max.index)
    return df_w_dummies_wo_minMax

def remove_test_time(df_w_dummies_wo_minMax):
    return df_w_dummies_wo_minMax.drop(['ITuff_PerUnit_testTimeInMS_Target'],axis=1)

def train_and_save_model(file_name, trainingDataPath, y_train, model, x_train_wo_LOT):
    model.fit(x_train_wo_LOT, y_train)

    # export model coefficient to file
    model.save_model(os.path.join(trainingDataPath, file_name + '.model.json'))

def export_columns(file_name, trainingDataPath, x_train_wo_LOT):
    column_names = x_train_wo_LOT.columns.tolist()

    # Save the column names to a text file
    with open(os.path.join(trainingDataPath, file_name + '.columns'), 'w') as file:
        for column in column_names:
            file.write(column + '\n')

def plot_actual_vs_predicted(y_test, y_pred):
    plt.scatter(y_test, y_pred)
    plt.plot([min(y_test), max(y_test)], [min(y_pred), max(y_pred)],'--', color='red')
    plt.xlabel('Actual values (y_test)')
    plt.ylabel('Predicted values (y_pred)')
    plt.title('Actual vs. Predicted Values')
    plt.show()

def print_statistics(y_test, y_pred):
    from sklearn.metrics import mean_squared_error, r2_score, mean_absolute_error

    mse = mean_squared_error(y_test, y_pred)
    rmse = np.sqrt(mse)
    r2 = r2_score(y_test, y_pred)
    mae = mean_absolute_error(y_test, y_pred)

    # print the results
    print("MSE:\t\t", '{:.2f}'.format(mse))
    print("RMSE:\t\t", '{:.2f}'.format(rmse))
    print("R-squared:\t", '{:.2f}'.format(r2))
    print("MAE:\t\t", '{:.2f}'.format(mae))

def plot_prediction_improvement_on_default_value(y_test, y_pred):
    default_test_time_value = 150
    pred_test_diff = abs(y_test - y_pred)
    pred_test_diff_default = abs(y_test - default_test_time_value)

    categories = ['Actual-Predicted Avg Diff', 'Actual-default Avg diff']
    values = [np.mean(pred_test_diff), np.mean(pred_test_diff_default)]

    # Creating the bar plot
    plt.bar(categories, values)

    # Adding labels and title
    plt.xlabel('Categories')
    plt.ylabel('Time in seconds')
    plt.title('Average Distance Improvment')

    for i, v in enumerate(values):
        plt.text(i, v,'%d' % v, ha='center', va='bottom')
    
    # Displaying the plot
    plt.show()

###########################

configure_graph_display(True)

file_name, file_csv_ext, trainingDataPath, df = load_training_data()
            
plot_tests_passed_failed(df)

plot_all_tests_time_histogram(df, max_time_filter = 1000)

plot_failed_tests_time_histogram(df, max_time_filter=1000)

# Call the function
plot_success_tests_time_histogram(df, max_time_filter=1000)

filtered_by_min_and_max = plot_test_time_categories(df)

df_w_dummies = create_dummies(df)

df_w_dummies_wo_minMax = drop_below_min_and_above_max(filtered_by_min_and_max, df_w_dummies)

df_w_dummies_wo_minMax = drop_unwanted_columns(df_w_dummies_wo_minMax)

df_w_dummies_w_minMax = drop_unwanted_columns(df_w_dummies)

#df_w_dummies_wo_minMax.info()
#df_w_dummies_wo_minMax.describe()

x_wo_minMax = remove_test_time(df_w_dummies_wo_minMax)
x_w_minMax = remove_test_time(df_w_dummies_w_minMax)

y_wo_minMax = df_w_dummies_wo_minMax.ITuff_PerUnit_testTimeInMS_Target
y_w_minMax = df_w_dummies_w_minMax.ITuff_PerUnit_testTimeInMS_Target

x_train, x_test, y_train, y_test = mods.train_test_split(x_wo_minMax, y_wo_minMax, test_size=0.30,random_state=101)
#x_train, x_test, y_train, y_test = mods.train_test_split(x_w_minMax, y_w_minMax, test_size=0.30,random_state=101)

# %%
from sklearn.linear_model import Ridge
from sklearn.linear_model import Lasso
from sklearn.linear_model import ElasticNet
from sklearn.linear_model import BayesianRidge
from xgboost import XGBRegressor

#model = sklin.LinearRegression()
#model = Ridge(alpha=1.0)
#model = Lasso(alpha=0.1)
#model  = ElasticNet(alpha=0.1, l1_ratio=0.5)
#model = BayesianRidge()
model = XGBRegressor()

x_train_wo_LOT, x_test_wo_LOT, x_with_minMax_wo_LOT = drop_unneeded_columns(x_w_minMax, x_train, x_test)

train_and_save_model(file_name, trainingDataPath, y_train, model, x_train_wo_LOT)

export_columns(file_name, trainingDataPath, x_train_wo_LOT)      

y_pred = model.predict(x_test_wo_LOT)

# %%
#y_pred_w_minMax = model.predict(x_with_minMax_wo_LOT)

# %%
# test_pred_df = pd.DataFrame({'vpo': x['ITuff_Lot_NA'],'y_test':y, 'y_pred': y_pred_w_minMax})
# test_pred_df.to_csv(file_name + '_withPred' + file_ext)

#test_pred_df_w_minMax = pd.DataFrame({'vpo': x_w_minMax['ITuff_Lot_NA'],'y_test':y_w_minMax, 'y_pred': y_pred_w_minMax})
#test_pred_df_w_minMax.to_csv(file_name + '_withPredComplete' + file_csv_ext)

y_check = y_pred / y_test * 100

plot_predict_vs_actual(y_check)

plot_accuracy_by_ranges(y_check, x_train_wo_LOT)

plot_actual_vs_predicted(y_test, y_pred)

print_statistics(y_test, y_pred)

plot_prediction_improvement_on_default_value(y_test, y_pred)
