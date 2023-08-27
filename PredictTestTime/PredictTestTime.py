import sys
import json

# read input data

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

data = {
    "name": sys.argv[1],
    "id": sys.argv[2]
}

# Serialize the dictionary to a JSON string
json_string = json.dumps(data, indent=4)  # The 'indent' parameter adds indentation for readability

# Print the JSON string
print(json_string)
