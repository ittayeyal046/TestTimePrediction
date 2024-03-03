import sys
import json

# read input data
parameters = json.loads(sys.argv[1])

# read train data

# combine train and input records

# create dummies

# remove train data



# load model parameters
model2 = XGBRegressor()
model2.load_model('C:/temp/model_parameters.model')

# run model on input data
prediction = model2.predict(inputData)

# export output as json to c# process as std out
print(12.345)
