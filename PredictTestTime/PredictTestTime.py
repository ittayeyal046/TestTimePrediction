import sys
import json

# read input data

# read train data

# combine train and input records

# create dummies

# remove train data

# load model parameters
#model2 = XGBRegressor()
#model2.load_model('C:/temp/model_parameters.model')

# run model on input data
#y_pred2 = model2.predict(inputData)

# export output as json to c# process



# The first command-line argument is the script name itself, so we start from index 1
# Loop through the arguments and print each one
#for i, arg in enumerate(sys.argv[1:], start=1):
#    print(f"Argument {i}: {arg}")

#_arg = '{"Name":"ittay","Id":"1234567"}'
#print(sys.argv[1])

parsed_dict = json.loads(sys.argv[1])

# Print the JSON string
print(parsed_dict)
