using System;
using System.Drawing;
using System.Windows.Forms;

public partial class MainForm : Form
{
    private ComboBox? comboBoxScreens;
    private Button? btnStartCapture;
    private Button? btnStopCapture;
    private Label? lblStatus;
    private int selectedScreenIndex = 0;

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

        // Start live screen capture
        LiveScreenCapture.StartLiveScreenCapture(selectedScreenIndex);

        if (lblStatus == null)
        {
            Console.WriteLine("Status label is not initialized. Cannot start capturing.");
            return;
        }

        lblStatus.Text = "Capturing... Press 'Q' in the console to stop.";
    }

    private void btnStopCapture_Click(object? sender, EventArgs e)
    {
        // Stop the live screen capture
        LiveScreenCapture.StopLiveScreenCapture();

        if (lblStatus == null)
        {
            MessageBox.Show("Status label is not initialized.");
            return;
        }

        lblStatus.Text = "Capture stopped.";
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

        // lblStatus
        this.lblStatus.AutoSize = true;
        this.lblStatus.Location = new Point(12, 65);
        this.lblStatus.Name = "lblStatus";
        this.lblStatus.Size = new Size(55, 13);
        this.lblStatus.TabIndex = 3;
        this.lblStatus.Text = "Status: Idle";

        // MainForm
        this.ClientSize = new Size(284, 101);
        this.Controls.Add(this.lblStatus);
        this.Controls.Add(this.btnStopCapture);
        this.Controls.Add(this.btnStartCapture);
        this.Controls.Add(this.comboBoxScreens);
        this.Name = "MainForm";
        this.Text = "Live Screen Capture";
    }
}
