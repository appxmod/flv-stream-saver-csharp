import subprocess
import time


consoleExe = r'bin/Debug/net6.0/FastStreamFix.exe'
# input_data = "Hello from Python\nAnother line\n"
path = r'....flv'

input_file = open(path, 'rb') 

buffer_size = 4096

# Start the C# console application
output_process = subprocess.Popen([
        consoleExe
        , '-i', 'pipe:0'
        , '-c', 'copy'
        , '-f', 'flv'
        , "R:\\Cache\\test_output.flv"
]
    , stdin=subprocess.PIPE
    , text=True)

# Write data to the stdin of the C# process
# output_process.stdin.write(input_data)
# output_process.stdin.close()

print('st...', output_process)


while 1:
    chunk = input_file.read(buffer_size)
    print('piping...', len(chunk))
    if not chunk:
        break
    output_process.stdin.buffer.write(chunk)
#     time.sleep(3.5)

output_process.stdin.close();
# Wait for the process to complete
output_process.wait()