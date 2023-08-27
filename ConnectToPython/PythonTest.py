import sys
import json

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
