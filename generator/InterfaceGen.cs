// GtkSharp.Generation.InterfaceGen.cs - The Interface Generatable.
//
// Author: Mike Kestner <mkestner@speakeasy.net>
//
// Copyright (c) 2001-2003 Mike Kestner
//
// This program is free software; you can redistribute it and/or
// modify it under the terms of version 2 of the GNU General Public
// License as published by the Free Software Foundation.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// General Public License for more details.
//
// You should have received a copy of the GNU General Public
// License along with this program; if not, write to the
// Free Software Foundation, Inc., 59 Temple Place - Suite 330,
// Boston, MA 02111-1307, USA.


namespace GtkSharp.Generation {

	using System;
	using System.IO;
	using System.Xml;

	public class InterfaceGen : ClassBase, IGeneratable  {

		public InterfaceGen (XmlElement ns, XmlElement elem) : base (ns, elem) {}

		public void Generate ()
		{
			GenerationInfo gen_info = new GenerationInfo (NSElem);
			Generate (gen_info);
		}

		public void Generate (GenerationInfo gen_info)
		{
			StreamWriter sw = gen_info.Writer = gen_info.OpenStream (Name);

			sw.WriteLine ("namespace " + NS + " {");
			sw.WriteLine ();
			sw.WriteLine ("\tusing System;");
			sw.WriteLine ();
			sw.WriteLine ("#region Autogenerated code");
			sw.WriteLine ("\tpublic interface " + Name + " : GLib.IWrapper {");
			sw.WriteLine ();
			
			foreach (Signal sig in sigs.Values) {
				if (sig.Validate ()) {
					sig.GenerateDecl (sw);
					sig.GenEventHandler (gen_info);
				}
			}

			foreach (Method method in methods.Values) {
				if (IgnoreMethod (method))
					continue;

				if (method.Validate ())
					method.GenerateDecl (sw);
			}

			AppendCustom (sw, gen_info.CustomDir);

			sw.WriteLine ("\t}");
			sw.WriteLine ("#endregion");
			sw.WriteLine ("}");
			sw.Close ();
			gen_info.Writer = null;
			Statistics.IFaceCount++;
		}
	}
}

