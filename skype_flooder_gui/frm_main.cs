﻿using System;
using System.Windows.Forms;
using SKYPE4COMLib;
using System.Text.RegularExpressions;
using System.Threading;
namespace skype_flooder_gui {
	public partial class frm_main : Form {
		Thread thr;
		private bool _attacking;
		public bool Attacking {
			get {
				return _attacking;
			}
			set {
				if ( value & !_attacking && !thr.IsAlive )
					run_flooder();
				_attacking = value;
				btn_flooding.Text = value ? "Stop" : "Flood!";
			}
		}
		void run_flooder() {
			( this.thr = new Thread( new ThreadStart( AttackProcess ) ) ).Start();
		}
		public frm_main() {
			//int protocol = Convert.ToInt32( skype_protokol.Value );
			//skype.Attach( protocol, false );
			
			InitializeComponent();
		}
		private void on_load( object sender, EventArgs e ) {
			try {
				Skype skype = new SkypeClass();//skype
				skype.Attach( Convert.ToInt32( skype_protokol.Value ), false );
				Regex r = new Regex( @"xmpp\:.*" );
				foreach ( User friend in skype.Friends ) {
					if ( !r.IsMatch( friend.Handle ) )
						list_users.Items.Add( new skype_friends() {
							s_user = friend
						} );
				}
			}
			catch {
				new frm_warning().ShowDialog();
			}
		}
		private void run_stop_attack( object sender, EventArgs e ) {
			this.Attacking ^= true;
		}
		private void AttackProcess() {
			Skype skype = new SkypeClass();//skype
			string target="", message="";
			int delay = 0, flood_type = 0,protocol=0;
			this.Invoke((Action)(()=>{
				target = this.txt_target.Text;
				message = txt_flood_text.Text;
				delay = Convert.ToInt32( this.nud_delay.Value )*1000;
				flood_type = this.rb_infinite_messages.Checked ? 0 : this.rb_timeout_messages.Checked ? 1 : 2;
				protocol = Convert.ToInt32( skype_protokol.Value );
			}));
			if ( message.Length == 0 ) {
				this.Invoke( (Action)( () => Attacking = false ) );
				return;
			}
			skype.Attach( protocol, false );
			if ( flood_type == 2 ) {
				try {
					skype.SendMessage( target, message );
					this.Invoke( (Action)( () => Attacking = false ) );
				}
				catch {}
				return;
			}
			while ( this.Attacking ) {
				try {
					skype.SendMessage( target, message );
				}
				catch {
				}
				if ( flood_type == 1 )
					Thread.Sleep( delay );
			}
		}
	}
}
