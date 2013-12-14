using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Text.RegularExpressions;
using System.Reflection;
using System.Globalization;

using WWActorEdit.Kazari;
using WWActorEdit.Kazari.DZx;
using WWActorEdit.Kazari.DZB;
using WWActorEdit.Kazari.J3Dx;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace WWActorEdit.Kazari
{
    public partial class ZAControl : UserControl
    {
        ZeldaArc Actor;

        public ZAControl(ZeldaArc ThisActor)
        {
            InitializeComponent();

            Actor = ThisActor;

            this.SuspendLayout();
            UpdateControl();
            this.ResumeLayout();
        }

        void UpdateControl()
        {
            AttachDetachEvents(false);
            
            numericUpDown1.Value = (decimal)Actor.Translation.X;
            numericUpDown2.Value = (decimal)Actor.Translation.Y;
            numericUpDown3.Value = (decimal)Actor.Translation.Z;

            
            numericUpDown4.Value = (decimal)Actor.Rotation.X;
            
            AttachDetachEvents(true);
        }

        void AttachDetachEvents(bool Attach)
        {
            if (Attach == true)
            {
                numericUpDown1.ValueChanged += new EventHandler(numericUpDown1_ValueChanged);
                numericUpDown2.ValueChanged += new EventHandler(numericUpDown2_ValueChanged);
                numericUpDown3.ValueChanged += new EventHandler(numericUpDown3_ValueChanged);
                numericUpDown4.ValueChanged += new EventHandler(numericUpDown4_ValueChanged);
            }
            else
            {
                numericUpDown1.ValueChanged -= new EventHandler(numericUpDown1_ValueChanged);
                numericUpDown2.ValueChanged -= new EventHandler(numericUpDown2_ValueChanged);
                numericUpDown3.ValueChanged -= new EventHandler(numericUpDown3_ValueChanged);
                numericUpDown4.ValueChanged -= new EventHandler(numericUpDown4_ValueChanged);
            }
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            Actor.ChangeTranslation(new OpenTK.Vector3((float)numericUpDown1.Value, Actor.Translation.Y, Actor.Translation.Z));
        }

        private void numericUpDown2_ValueChanged(object sender, EventArgs e)
        {
            Actor.ChangeTranslation(new OpenTK.Vector3(Actor.Translation.X, (float)numericUpDown2.Value, Actor.Translation.Z));
        }

        private void numericUpDown3_ValueChanged(object sender, EventArgs e)
        {
            Actor.ChangeTranslation(new OpenTK.Vector3(Actor.Translation.X, Actor.Translation.Y, (float)numericUpDown3.Value));
        }

        private void numericUpDown4_ValueChanged(object sender, EventArgs e)
        {
            Actor.ChangeRotation(new OpenTK.Vector3((float)numericUpDown4.Value, Actor.Rotation.Y, Actor.Rotation.Z));
        }

        private void label3_Click(object sender, EventArgs e)
        {
        }

        private void label4_Click(object sender, EventArgs e)
        {
        }
    }
}
