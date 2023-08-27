import sys

# The first command-line argument is the script name itself, so we start from index 1
# Loop through the arguments and print each one
for i, arg in enumerate(sys.argv[1:], start=1):
    print(f"Argument {i}: {arg}")
