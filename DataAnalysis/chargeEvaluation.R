# Map 1-based optional input ports to variables
dataset1 <- maml.mapInputPort(1) # class: data.frame
# dataset2 <- maml.mapInputPort(2) # class: data.frame

dt = data.frame(dataset1)
dateStamp = substr(as.character(dt$DateStamp), 1, 19)
dateStamp = strptime(dateStamp, format = "%Y-%m-%d %H:%M:%S", tz = "America/Chicago")
x = as.numeric(dateStamp-dateStamp[1])
y = dt$BatteryCurrent
plot(x, y)
IoC = c(0)
remainder = c(75*3600)
for (i in c(1:(length(y)-1))) {
  diff = x[i+1] - x[i]
  avg = (y[i] + y[i+1]) / 2
  IoC[i+1] = diff * avg + IoC[i]
  remainder[i+1] = 75*3600 - IoC[i+1]
}
# remainder = data.frame(remainder)
# maml.mapOutputPort("remainder");

timeInput = maml.mapInputPort(2) #time
timeInput = timeInput$DateTime[1]
# str(timeInput)
timeInput = as.character(timeInput)
timeInput = strptime(timeInput, format = "%Y-%m-%d %H:%M:%S", tz = "America/Chicago")
# attr(timeInput, "tzone") <- "America/Chicago"
# timeInput

# str(dateStamp[1])
# dateStamp[1]
index = 1
z = as.numeric(timeInput-dateStamp[1])
# z = z/60
if (z >= x[length(x)]) {
  index = length(x)
} else {
  for (i in c(1:length(x))) {
    if (z < x[i+1]) {
      index = i
      break;
    }
  }
}
remainder
index
remainder[index]
evaluation = remainder[index]
evaluation = data.frame(evaluation)
maml.mapOutputPort("evaluation"); # Charge evaluation
# maml.mapOutputPort("timeInput")