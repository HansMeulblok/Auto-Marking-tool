using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

public partial class MainForm : Form
{
    private ComboBox? comboBoxScreens;
    private Button? btnStartCapture;
    private Button? btnStopCapture;
    private Button? btnProcessSheet;

    private Label? lblStatus;
    private int selectedScreenIndex = 0;
    private CancellationTokenSource? cancellationTokenSource;

    public MainForm()
    {
        InitializeComponent();

        lblStatus.Text = "Status: Idle";

        this.Load += MainForm_Load;
    }

    private void MainForm_Load(object? sender, EventArgs e)
    {
        InitializeScreenDropdown();
    }

    private void InitializeScreenDropdown()
    {
        if (!this.Controls.Contains(comboBoxScreens))
        {
            this.Controls.Add(comboBoxScreens);
        }

        if (comboBoxScreens == null)
        {
            MessageBox.Show("Screen dropdown is not initialized.");
            return;
        }

        comboBoxScreens.Items.Clear();

        comboBoxScreens.Items.Add("Primary Screen");

        int screenCount = Screen.AllScreens.Length;
        Console.WriteLine($"Number of screens detected: {screenCount}");

        for (int i = 1; i < screenCount; i++)
        {
            comboBoxScreens.Items.Add($"Screen {i + 1}");
        }

        if (screenCount > 0)
        {
            comboBoxScreens.SelectedIndex = 0;
        }

        comboBoxScreens.SelectedIndexChanged += ComboBoxScreens_SelectedIndexChanged;
    }

    private void ComboBoxScreens_SelectedIndexChanged(object? sender, EventArgs e)
    {
        if (comboBoxScreens != null)
        {
            selectedScreenIndex = comboBoxScreens.SelectedIndex;
            Console.WriteLine($"Screen {selectedScreenIndex + 1} selected.");
        }
    }

    private void btnStartCapture_Click(object? sender, EventArgs e)
    {
        if (comboBoxScreens == null || comboBoxScreens.Items.Count == 0)
        {
            MessageBox.Show("Screen dropdown is not initialized or empty.");
            return;
        }

        if (selectedScreenIndex < 0)
        {
            MessageBox.Show("Please select a screen to capture.");
            return;
        }

        // Initialize cancellation token source
        cancellationTokenSource = new CancellationTokenSource();

        // Start live screen capture in a new task
        Task.Run(() =>
        {
            try
            {
                LiveScreenCapture.StartLiveScreenCapture(selectedScreenIndex);

                Invoke(new Action(() =>
                {
                    lblStatus.Text = "Capturing... Press 'Stop' to halt.";
                    btnStartCapture.Enabled = false;
                    btnStopCapture.Enabled = true;
                }));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during screen capture: {ex.Message}");
            }
        });
    }

    private void btnStopCapture_Click(object? sender, EventArgs e)
    {
        if (cancellationTokenSource != null)
        {
            // Cancel the ongoing operation
            cancellationTokenSource.Cancel();
            cancellationTokenSource.Dispose();
            cancellationTokenSource = null;

            // Stop the live screen capture
            LiveScreenCapture.StopLiveScreenCapture();

            Invoke(new Action(() =>
            {
                lblStatus.Text = "Capture stopped.";
                btnStartCapture.Enabled = true;
                btnStopCapture.Enabled = false;
            }));
        }
        else
        {
            MessageBox.Show("No active capture to stop.");
        }
    }

    private void BtnProcessSheet_Click(object? sender, EventArgs e)
    {
        if (cancellationTokenSource == null)
        {
            MessageBox.Show("Please start capturing before processing.");
            return;
        }

        Task.Run(() => ProcessScreenCaptureWithGoogleSheet(cancellationTokenSource.Token));
    }

    public void ProcessScreenCaptureWithGoogleSheet(CancellationToken token)
    {
        var sheetsHelper = new GoogleSheetsHelper();
        string spreadsheetId = "1QhI0b92LF2cnY_bay8Ds7pRcyoK0rSjCud3QzOB78dE";
        string range = "Sheet1!A1:A"; 
        int sheetId = 0; 

        try
        {
            while (true)
            {
                token.ThrowIfCancellationRequested();

                Bitmap screenshot = LiveScreenCapture.CaptureScreen(selectedScreenIndex);
                string recognizedText = TextRecognizer.ExtractTextFromImage(screenshot);

                if (!string.IsNullOrWhiteSpace(recognizedText))
                {
                    var lines = recognizedText.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

                    foreach (var line in lines)
                    {
                        var trimmedLine = line.Trim();
                        var cellLocation = sheetsHelper.FindCell(spreadsheetId, range, trimmedLine);

                        if (cellLocation.HasValue)
                        {
                            Console.WriteLine($"Text '{trimmedLine}' found in row {cellLocation.Value.Row + 1}.");
                            sheetsHelper.UpdateCellBackground(
                                spreadsheetId,
                                sheetId,
                                cellLocation.Value.Row,
                                cellLocation.Value.Column,
                                "#00FF00" // Green
                            );
                        }
                        else
                        {
                            Console.WriteLine($"Text '{trimmedLine}' not found in the sheet.");
                        }
                    }
                }

                Thread.Sleep(1000); // Process every second
            }
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Processing was canceled.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during processing: {ex.Message}");
        }
    }

    private void InitializeComponent()
    {
        this.comboBoxScreens = new ComboBox();
        this.btnStartCapture = new Button();
        this.btnStopCapture = new Button();
        this.lblStatus = new Label();

        // comboBoxScreens
        this.comboBoxScreens.DropDownStyle = ComboBoxStyle.DropDownList;
        this.comboBoxScreens.Location = new Point(12, 12);
        this.comboBoxScreens.Name = "comboBoxScreens";
        this.comboBoxScreens.Size = new Size(260, 21);

        // btnStartCapture
        this.btnStartCapture.Location = new Point(12, 39);
        this.btnStartCapture.Name = "btnStartCapture";
        this.btnStartCapture.Size = new Size(75, 23);
        this.btnStartCapture.TabIndex = 1;
        this.btnStartCapture.Text = "Start Capture";
        this.btnStartCapture.UseVisualStyleBackColor = true;
        this.btnStartCapture.Click += new EventHandler(btnStartCapture_Click);

        // btnStopCapture
        this.btnStopCapture.Location = new Point(197, 39);
        this.btnStopCapture.Name = "btnStopCapture";
        this.btnStopCapture.Size = new Size(75, 23);
        this.btnStopCapture.TabIndex = 2;
        this.btnStopCapture.Text = "Stop Capture";
        this.btnStopCapture.UseVisualStyleBackColor = true;
        this.btnStopCapture.Click += new EventHandler(btnStopCapture_Click);
        this.btnStopCapture.Enabled = false; // Initially disabled

        // lblStatus
        this.lblStatus.AutoSize = true;
        this.lblStatus.Location = new Point(12, 65);
        this.lblStatus.Name = "lblStatus";
        this.lblStatus.Size = new Size(55, 13);
        this.lblStatus.TabIndex = 3;
        this.lblStatus.Text = "Status: Idle";

        // btnProcessSheet
        this.btnProcessSheet = new Button();
        this.btnProcessSheet.Location = new Point(105, 95); // Adjusted position
        this.btnProcessSheet.Name = "btnProcessSheet";
        this.btnProcessSheet.Size = new Size(75, 23);
        this.btnProcessSheet.TabIndex = 4;
        this.btnProcessSheet.Text = "Process";
        this.btnProcessSheet.UseVisualStyleBackColor = true;
        this.btnProcessSheet.Click += new EventHandler(BtnProcessSheet_Click);

        // MainForm
        this.ClientSize = new Size(284, 130);
        this.Controls.Add(this.btnProcessSheet);
        this.Controls.Add(this.lblStatus);
        this.Controls.Add(this.btnStopCapture);
        this.Controls.Add(this.btnStartCapture);
        this.Controls.Add(this.comboBoxScreens);
        this.Name = "MainForm";
        this.Text = "Live Screen Capture";
    }
}
