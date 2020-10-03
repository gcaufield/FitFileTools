#region Copyright
////////////////////////////////////////////////////////////////////////////////
// The following FIT Protocol software provided may be used with FIT protocol
// devices only and remains the copyrighted property of Garmin Canada Inc.
// The software is being provided on an "as-is" basis and as an accommodation,
// and therefore all warranties, representations, or guarantees of any kind
// (whether express, implied or statutory) including, without limitation,
// warranties of merchantability, non-infringement, or fitness for a particular
// purpose, are specifically disclaimed.
//
// Copyright 2020 Garmin Canada Inc.
////////////////////////////////////////////////////////////////////////////////
// ****WARNING****  This file is auto-generated!  Do NOT edit this file.
// Profile Version = 21.38Release
// Tag = production/akw/21.38.00-0-g0d69e49
////////////////////////////////////////////////////////////////////////////////

#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.IO;
using System.Linq;

namespace Dynastream.Fit
{
    /// <summary>
    /// Implements the ExerciseTitle profile message.
    /// </summary>
    public class ExerciseTitleMesg : Mesg
    {
        #region Fields
        #endregion

        /// <summary>
        /// Field Numbers for <see cref="ExerciseTitleMesg"/>
        /// </summary>
        public sealed class FieldDefNum
        {
            public const byte MessageIndex = 254;
            public const byte ExerciseCategory = 0;
            public const byte ExerciseName = 1;
            public const byte WktStepName = 2;
            public const byte Invalid = Fit.FieldNumInvalid;
        }

        #region Constructors
        public ExerciseTitleMesg() : base(Profile.GetMesg(MesgNum.ExerciseTitle))
        {
        }

        public ExerciseTitleMesg(Mesg mesg) : base(mesg)
        {
        }
        #endregion // Constructors

        #region Methods
        ///<summary>
        /// Retrieves the MessageIndex field</summary>
        /// <returns>Returns nullable ushort representing the MessageIndex field</returns>
        public ushort? GetMessageIndex()
        {
            Object val = GetFieldValue(254, 0, Fit.SubfieldIndexMainField);
            if(val == null)
            {
                return null;
            }

            return (Convert.ToUInt16(val));

        }

        /// <summary>
        /// Set MessageIndex field</summary>
        /// <param name="messageIndex_">Nullable field value to be set</param>
        public void SetMessageIndex(ushort? messageIndex_)
        {
            SetFieldValue(254, 0, messageIndex_, Fit.SubfieldIndexMainField);
        }

        ///<summary>
        /// Retrieves the ExerciseCategory field</summary>
        /// <returns>Returns nullable ushort representing the ExerciseCategory field</returns>
        public ushort? GetExerciseCategory()
        {
            Object val = GetFieldValue(0, 0, Fit.SubfieldIndexMainField);
            if(val == null)
            {
                return null;
            }

            return (Convert.ToUInt16(val));

        }

        /// <summary>
        /// Set ExerciseCategory field</summary>
        /// <param name="exerciseCategory_">Nullable field value to be set</param>
        public void SetExerciseCategory(ushort? exerciseCategory_)
        {
            SetFieldValue(0, 0, exerciseCategory_, Fit.SubfieldIndexMainField);
        }

        ///<summary>
        /// Retrieves the ExerciseName field</summary>
        /// <returns>Returns nullable ushort representing the ExerciseName field</returns>
        public ushort? GetExerciseName()
        {
            Object val = GetFieldValue(1, 0, Fit.SubfieldIndexMainField);
            if(val == null)
            {
                return null;
            }

            return (Convert.ToUInt16(val));

        }

        /// <summary>
        /// Set ExerciseName field</summary>
        /// <param name="exerciseName_">Nullable field value to be set</param>
        public void SetExerciseName(ushort? exerciseName_)
        {
            SetFieldValue(1, 0, exerciseName_, Fit.SubfieldIndexMainField);
        }


        /// <summary>
        ///
        /// </summary>
        /// <returns>returns number of elements in field WktStepName</returns>
        public int GetNumWktStepName()
        {
            return GetNumFieldValues(2, Fit.SubfieldIndexMainField);
        }

        ///<summary>
        /// Retrieves the WktStepName field</summary>
        /// <param name="index">0 based index of WktStepName element to retrieve</param>
        /// <returns>Returns byte[] representing the WktStepName field</returns>
        public byte[] GetWktStepName(int index)
        {
            byte[] data = (byte[])GetFieldValue(2, index, Fit.SubfieldIndexMainField);
            return data.Take(data.Length - 1).ToArray();
        }

        ///<summary>
        /// Retrieves the WktStepName field</summary>
        /// <param name="index">0 based index of WktStepName element to retrieve</param>
        /// <returns>Returns String representing the WktStepName field</returns>
        public String GetWktStepNameAsString(int index)
        {
            byte[] data = (byte[])GetFieldValue(2, index, Fit.SubfieldIndexMainField);
            return data != null ? Encoding.UTF8.GetString(data, 0, data.Length - 1) : null;
        }

        ///<summary>
        /// Set WktStepName field</summary>
        /// <param name="index">0 based index of WktStepName element to retrieve</param>
        /// <param name="wktStepName_"> field value to be set</param>
        public void SetWktStepName(int index, String wktStepName_)
        {
            byte[] data = Encoding.UTF8.GetBytes(wktStepName_);
            byte[] zdata = new byte[data.Length + 1];
            data.CopyTo(zdata, 0);
            SetFieldValue(2, index, zdata, Fit.SubfieldIndexMainField);
        }


        /// <summary>
        /// Set WktStepName field</summary>
        /// <param name="index">0 based index of wkt_step_name</param>
        /// <param name="wktStepName_">field value to be set</param>
        public void SetWktStepName(int index, byte[] wktStepName_)
        {
            SetFieldValue(2, index, wktStepName_, Fit.SubfieldIndexMainField);
        }

        #endregion // Methods
    } // Class
} // namespace
