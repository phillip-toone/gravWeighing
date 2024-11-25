<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmBlankPicker
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
      Me.Label1 = New System.Windows.Forms.Label
      Me.lblInstruction = New System.Windows.Forms.Label
      Me.btnRecord = New System.Windows.Forms.Button
      Me.txbBlank = New System.Windows.Forms.TextBox
      Me.txbSet = New System.Windows.Forms.TextBox
      Me.Label3 = New System.Windows.Forms.Label
      Me.Label4 = New System.Windows.Forms.Label
      Me.Label2 = New System.Windows.Forms.Label
      Me.Panel1 = New System.Windows.Forms.Panel
      Me.Panel2 = New System.Windows.Forms.Panel
      Me.Label5 = New System.Windows.Forms.Label
      Me.MenuStrip1 = New System.Windows.Forms.MenuStrip
      Me.ExperimentToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem
      Me.mnuSave = New System.Windows.Forms.ToolStripMenuItem
      Me.mnuReset = New System.Windows.Forms.ToolStripMenuItem
      Me.Panel3 = New System.Windows.Forms.Panel
      Me.Label6 = New System.Windows.Forms.Label
      Me.Panel1.SuspendLayout()
      Me.Panel2.SuspendLayout()
      Me.MenuStrip1.SuspendLayout()
      Me.Panel3.SuspendLayout()
      Me.SuspendLayout()
      '
      'Label1
      '
      Me.Label1.AutoSize = True
      Me.Label1.Font = New System.Drawing.Font("Microsoft Sans Serif", 14.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
      Me.Label1.Location = New System.Drawing.Point(28, 32)
      Me.Label1.Name = "Label1"
      Me.Label1.Size = New System.Drawing.Size(237, 24)
      Me.Label1.TabIndex = 0
      Me.Label1.Text = "Filter Blank Identification"
      '
      'lblInstruction
      '
      Me.lblInstruction.AutoSize = True
      Me.lblInstruction.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
      Me.lblInstruction.Location = New System.Drawing.Point(312, 28)
      Me.lblInstruction.Name = "lblInstruction"
      Me.lblInstruction.Size = New System.Drawing.Size(385, 13)
      Me.lblInstruction.TabIndex = 1
      Me.lblInstruction.Text = "Click on the first set of Filter Blanks, then click the Record button."
      '
      'btnRecord
      '
      Me.btnRecord.BackColor = System.Drawing.Color.Yellow
      Me.btnRecord.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
      Me.btnRecord.Location = New System.Drawing.Point(568, 76)
      Me.btnRecord.Name = "btnRecord"
      Me.btnRecord.Size = New System.Drawing.Size(144, 36)
      Me.btnRecord.TabIndex = 2
      Me.btnRecord.Text = "Record Blanks"
      Me.btnRecord.UseVisualStyleBackColor = False
      '
      'txbBlank
      '
      Me.txbBlank.Location = New System.Drawing.Point(568, 152)
      Me.txbBlank.Multiline = True
      Me.txbBlank.Name = "txbBlank"
      Me.txbBlank.Size = New System.Drawing.Size(56, 152)
      Me.txbBlank.TabIndex = 3
      '
      'txbSet
      '
      Me.txbSet.Location = New System.Drawing.Point(656, 152)
      Me.txbSet.Multiline = True
      Me.txbSet.Name = "txbSet"
      Me.txbSet.Size = New System.Drawing.Size(56, 496)
      Me.txbSet.TabIndex = 4
      '
      'Label3
      '
      Me.Label3.AutoSize = True
      Me.Label3.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
      Me.Label3.Location = New System.Drawing.Point(572, 136)
      Me.Label3.Name = "Label3"
      Me.Label3.Size = New System.Drawing.Size(45, 13)
      Me.Label3.TabIndex = 5
      Me.Label3.Text = "Blanks"
      '
      'Label4
      '
      Me.Label4.AutoSize = True
      Me.Label4.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
      Me.Label4.Location = New System.Drawing.Point(656, 136)
      Me.Label4.Name = "Label4"
      Me.Label4.Size = New System.Drawing.Size(58, 13)
      Me.Label4.TabIndex = 6
      Me.Label4.Text = "Filter Set"
      '
      'Label2
      '
      Me.Label2.AutoSize = True
      Me.Label2.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
      Me.Label2.Location = New System.Drawing.Point(28, 12)
      Me.Label2.Name = "Label2"
      Me.Label2.Size = New System.Drawing.Size(71, 13)
      Me.Label2.TabIndex = 9
      Me.Label2.Text = "Filter Blank"
      '
      'Panel1
      '
      Me.Panel1.BackColor = System.Drawing.Color.Yellow
      Me.Panel1.Controls.Add(Me.Label2)
      Me.Panel1.Location = New System.Drawing.Point(516, 516)
      Me.Panel1.Name = "Panel1"
      Me.Panel1.Size = New System.Drawing.Size(124, 36)
      Me.Panel1.TabIndex = 10
      '
      'Panel2
      '
      Me.Panel2.BackColor = System.Drawing.Color.Lime
      Me.Panel2.Controls.Add(Me.Label5)
      Me.Panel2.Location = New System.Drawing.Point(516, 564)
      Me.Panel2.Name = "Panel2"
      Me.Panel2.Size = New System.Drawing.Size(124, 36)
      Me.Panel2.TabIndex = 11
      '
      'Label5
      '
      Me.Label5.AutoSize = True
      Me.Label5.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
      Me.Label5.Location = New System.Drawing.Point(4, 12)
      Me.Label5.Name = "Label5"
      Me.Label5.Size = New System.Drawing.Size(109, 13)
      Me.Label5.TabIndex = 9
      Me.Label5.Text = "Assigned to Blank"
      '
      'MenuStrip1
      '
      Me.MenuStrip1.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.ExperimentToolStripMenuItem})
      Me.MenuStrip1.Location = New System.Drawing.Point(0, 0)
      Me.MenuStrip1.Name = "MenuStrip1"
      Me.MenuStrip1.Size = New System.Drawing.Size(767, 24)
      Me.MenuStrip1.TabIndex = 12
      Me.MenuStrip1.Text = "MenuStrip1"
      '
      'ExperimentToolStripMenuItem
      '
      Me.ExperimentToolStripMenuItem.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.mnuSave, Me.mnuReset})
      Me.ExperimentToolStripMenuItem.Name = "ExperimentToolStripMenuItem"
      Me.ExperimentToolStripMenuItem.Size = New System.Drawing.Size(73, 20)
      Me.ExperimentToolStripMenuItem.Text = "Experiment"
      '
      'mnuSave
      '
      Me.mnuSave.Name = "mnuSave"
      Me.mnuSave.Size = New System.Drawing.Size(152, 22)
      Me.mnuSave.Text = "Save && Exit"
      '
      'mnuReset
      '
      Me.mnuReset.Name = "mnuReset"
      Me.mnuReset.Size = New System.Drawing.Size(152, 22)
      Me.mnuReset.Text = "Reset Form"
      '
      'Panel3
      '
      Me.Panel3.BackColor = System.Drawing.Color.Blue
      Me.Panel3.Controls.Add(Me.Label6)
      Me.Panel3.Location = New System.Drawing.Point(516, 612)
      Me.Panel3.Name = "Panel3"
      Me.Panel3.Size = New System.Drawing.Size(124, 36)
      Me.Panel3.TabIndex = 13
      '
      'Label6
      '
      Me.Label6.AutoSize = True
      Me.Label6.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
      Me.Label6.ForeColor = System.Drawing.Color.White
      Me.Label6.Location = New System.Drawing.Point(24, 12)
      Me.Label6.Name = "Label6"
      Me.Label6.Size = New System.Drawing.Size(80, 13)
      Me.Label6.TabIndex = 9
      Me.Label6.Text = "Uncommitted"
      '
      'frmBlankPicker
      '
      Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
      Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
      Me.BackColor = System.Drawing.Color.MintCream
      Me.ClientSize = New System.Drawing.Size(767, 664)
      Me.Controls.Add(Me.Panel3)
      Me.Controls.Add(Me.Panel2)
      Me.Controls.Add(Me.Panel1)
      Me.Controls.Add(Me.Label4)
      Me.Controls.Add(Me.Label3)
      Me.Controls.Add(Me.txbSet)
      Me.Controls.Add(Me.txbBlank)
      Me.Controls.Add(Me.btnRecord)
      Me.Controls.Add(Me.lblInstruction)
      Me.Controls.Add(Me.Label1)
      Me.Controls.Add(Me.MenuStrip1)
      Me.MainMenuStrip = Me.MenuStrip1
      Me.Name = "frmBlankPicker"
      Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
      Me.Text = "Filter Blank Indentification"
      Me.Panel1.ResumeLayout(False)
      Me.Panel1.PerformLayout()
      Me.Panel2.ResumeLayout(False)
      Me.Panel2.PerformLayout()
      Me.MenuStrip1.ResumeLayout(False)
      Me.MenuStrip1.PerformLayout()
      Me.Panel3.ResumeLayout(False)
      Me.Panel3.PerformLayout()
      Me.ResumeLayout(False)
      Me.PerformLayout()

   End Sub
   Friend WithEvents Label1 As System.Windows.Forms.Label
   Friend WithEvents lblInstruction As System.Windows.Forms.Label
   Friend WithEvents btnRecord As System.Windows.Forms.Button
   Friend WithEvents txbBlank As System.Windows.Forms.TextBox
   Friend WithEvents txbSet As System.Windows.Forms.TextBox
   Friend WithEvents Label3 As System.Windows.Forms.Label
   Friend WithEvents Label4 As System.Windows.Forms.Label
   Friend WithEvents Label2 As System.Windows.Forms.Label
   Friend WithEvents Panel1 As System.Windows.Forms.Panel
   Friend WithEvents Panel2 As System.Windows.Forms.Panel
   Friend WithEvents Label5 As System.Windows.Forms.Label
   Friend WithEvents MenuStrip1 As System.Windows.Forms.MenuStrip
   Friend WithEvents ExperimentToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
   Friend WithEvents mnuSave As System.Windows.Forms.ToolStripMenuItem
   Friend WithEvents mnuReset As System.Windows.Forms.ToolStripMenuItem
   Friend WithEvents Panel3 As System.Windows.Forms.Panel
   Friend WithEvents Label6 As System.Windows.Forms.Label
End Class
