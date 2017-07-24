library(ggplot2)
library(reshape2)

csv_file <- "Data\\pam_data.csv"
output   <- "Plots\\hplot.png"

if (!file.exists(csv_file)) {
  stop(paste("No data to plot,", csv_file, "does not exist."))
}

# Read data from csv coverting variable columns.
df <- melt(read.csv(csv_file), id.vars = "Date", variable.name = "activities")

df$Date  <- as.Date(df$Date)
df$value <- df$value / 60

plt <- ggplot(data = df, aes(x = Date, y = value, group = activities, fill = activities)) +
  geom_col() +
  scale_y_continuous(breaks = scales::pretty_breaks(n = 10)) +
  labs(
    x     = "",
    y     = "Hours",
    title = "PAM Data",
    fill  = "Activity"
  )

ggsave(filename = output, plot = plt, device = "png", dpi = 128, height = 9, width = 14)
