<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmExperimentBuilder
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
      Me.dgvExperiment = New System.Windows.Forms.DataGridView
      Me.Column1 = New System.Windows.Forms.DataGridViewComboBoxColumn
      Me.Column2 = New System.Windows.Forms.DataGridViewTextBoxColumn
      Me.Column3 = New System.Windows.Forms.DataGridViewTextBoxColumn
      Me.rdoTare = New System.Windows.Forms.RadioButton
      Me.rdoWeight = New System.Windows.Forms.RadioButton
      Me.btnRun = New System.Windows.Forms.Button
      Me.lblNotice = New System.Windows.Forms.Label
      Me.rdoUserDefined = New System.Windows.Forms.RadioButton
      Me.btnSelect = New System.Windows.Forms.Button
      Me.lblSelect = New System.Windows.Forms.Label
      CType(Me.dgvExperiment, System.ComponentModel.ISupportInitialize).BeginInit()
      Me.SuspendLayout()
      '
      'dgvExperiment
      '
      Me.dgvExperiment.AllowUserToAddRows = False
      Me.dgvExperiment.AllowUserToDeleteRows = False
      Me.dgvExperiment.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
      Me.dgvExperiment.Columns.AddRange(New System.Windows.Forms.DataGridViewColumn() {Me.Column1, Me.Column2, Me.Column3})
      Me.dgvExperiment.EditMode = System.Windows.Forms.DataGridViewEditMode.EditOnEnter
      Me.dgvExperiment.Location = New System.Drawing.Point(20, 92)
      Me.dgvExperiment.Name = "dgvExperiment"
      Me.dgvExperiment.Size = New System.Drawing.Size(504, 212)
      Me.dgvExperiment.TabIndex = 1
      '
      'Column1
      '
      Me.Column1.HeaderText = "Task"
      Me.Column1.Items.AddRange(New Object() {"None", "Take Filter to Oven", "Remove Filter From Oven", "Weigh Filter & Return to Oven", "Weigh Filter & Return to Carousel", "Delay"})
      Me.Column1.Name = "Column1"
      Me.Column1.Width = 200
      '
      'Column2
      '
      Me.Column2.HeaderText = "Step Duration (Hr)"
      Me.Column2.Name = "Column2"
      Me.Column2.Width = 120
      '
      'Column3
      '
      Me.Column3.HeaderText = "Temperature (oC)"
      Me.Column3.Name = "Column3"
      Me.Column3.Width = 120
      '
      'rdoTare
      '
      Me.rdoTare.AutoSize = True
      Me.rdoTare.Font = New System.Drawing.Font("Microsoft Sans Serif", 12.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
      Me.rdoTare.Location = New System.Drawing.Point(24, 24)
      Me.rdoTare.Name = "rdoTare"
      Me.rdoTare.Size = New System.Drawing.Size(158, 24)
      Me.rdoTare.TabIndex = 2
      Me.rdoTare.Text = "Tare Experiment"
      Me.rdoTare.UseVisualStyleBackColor = True
      '
      'rdoWeight
      '
      Me.rdoWeight.AutoSize = True
      Me.rdoWeight.Font = New System.Drawing.Font("Microsoft Sans Serif", 12.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
      Me.rdoWeight.Location = New System.Drawing.Point(224, 24)
      Me.rdoWeight.Name = "rdoWeight"
      Me.rdoWeight.Size = New System.Drawing.Size(178, 24)
      Me.rdoWeight.TabIndex = 3
      Me.rdoWeight.TabStop = True
      Me.rdoWeight.Text = "Weight Experiment"
      Me.rdoWeight.UseVisualStyleBackColor = True
      '
      'btnRun
      '
      Me.btnRun.BackColor = System.Drawing.Color.Yellow
      Me.btnRun.Location = New System.Drawing.Point(556, 92)
      Me.btnRun.Name = "btnRun"
      Me.btnRun.Size = New System.Drawing.Size(100, 48)
      Me.btnRun.TabIndex = 4
      Me.btnRun.Text = "Load and Run Experiement"
      Me.btnRun.UseVisualStyleBackColor = False
      '
      'lblNotice
      '
      Me.lblNotice.AutoSize = True
      Me.lblNotice.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
      Me.lblNotice.Location = New System.Drawing.Point(24, 56)
      Me.lblNotice.Name = "lblNotice"
      Me.lblNotice.Size = New System.Drawing.Size(264, 26)
      Me.lblNotice.TabIndex = 5
      Me.lblNotice.Text = "NOTE:   Experiments must include at least 4" & Global.Microsoft.VisualBasic.ChrW(13) & Global.Microsoft.VisualBasic.ChrW(10) & "weighiing steps to ensure correct pr" & _
          "ocessing."
      '
      'rdoUserDefined
      '
      Me.rdoUserDefined.AutoSize = True
      Me.rdoUserDefined.Font = New System.Drawing.Font("Microsoft Sans Serif", 12.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
      Me.rdoUserDefined.Location = New System.Drawing.Point(456, 24)
      Me.rdoUserDefined.Name = "rdoUserDefined"
      Me.rdoUserDefined.Size = New System.Drawing.Size(228, 24)
      Me.rdoUserDefined.TabIndex = 6
      Me.rdoUserDefined.TabStop = True
      Me.rdoUserDefined.Text = "User Defined Experiment"
      Me.rdoUserDefined.UseVisualStyleBackColor = True
      '
      'btnSelect
      '
      Me.btnSelect.BackColor = System.Drawing.Color.LawnGreen
      Me.btnSelect.Location = New System.Drawing.Point(700, 24)
      Me.btnSelect.Name = "btnSelect"
      Me.btnSelect.Size = New System.Drawing.Size(144, 24)
      Me.btnSelect.TabIndex = 7
      Me.btnSelect.Text = "Select Specific Filters"
      Me.btnSelect.UseVisualStyleBackColor = False
      Me.btnSelect.Visible = False
      '
      'lblSelect
      '
      Me.lblSelect.AutoSize = True
      Me.lblSelect.Location = New System.Drawing.Point(856, 24)
      Me.lblSelect.Name = "lblSelect"
      Me.lblSelect.Size = New System.Drawing.Size(129, 26)
      Me.lblSelect.TabIndex = 8
      Me.lblSelect.Text = "Click on the filter positions" & Global.Microsoft.VisualBasic.ChrW(13) & Global.Microsoft.VisualBasic.ChrW(10) & "you want to enable."
      Me.lblSelect.Visible = False
      '
      'frmExperimentBuilder
      '
      Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
      Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
      Me.ClientSize = New System.Drawing.Size(1002, 578)
      Me.Controls.Add(Me.lblSelect)
      Me.Controls.Add(Me.btnSelect)
      Me.Controls.Add(Me.rdoUserDefined)
      Me.Controls.Add(Me.lblNotice)
      Me.Controls.Add(Me.btnRun)
      Me.Controls.Add(Me.rdoWeight)
      Me.Controls.Add(Me.rdoTare)
      Me.Controls.Add(Me.dgvExperiment)
      Me.Name = "frmExperimentBuilder"
      Me.Text = "Experiment Builder"
      CType(Me.dgvExperiment, System.ComponentModel.ISupportInitialize).EndInit()
      Me.ResumeLayout(False)
      Me.PerformLayout()

   End Sub
   Friend WithEvents dgvExperiment As System.Windows.Forms.DataGridView
   Friend WithEvents rdoTare As System.Windows.Forms.RadioButton
   Friend WithEvents rdoWeight As System.Windows.Forms.RadioButton
   Friend WithEvents btnRun As System.Windows.Forms.Button
   Friend WithEvents Column1 As System.Windows.Forms.DataGridViewComboBoxColumn
   Friend WithEvents Column2 As System.Windows.Forms.DataGridViewTextBoxColumn
   Friend WithEvents Column3 As System.Windows.Forms.DataGridViewTextBoxColumn
   Friend WithEvents lblNotice As System.Windows.Forms.Label
   Friend WithEvents rdoUserDefined As System.Windows.Forms.RadioButton
   Friend WithEvents btnSelect As System.Windows.Forms.Button
   Friend WithEvents lblSelect As System.Windows.Forms.Label
End Class
