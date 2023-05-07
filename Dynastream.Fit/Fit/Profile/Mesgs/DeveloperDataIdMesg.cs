#region Copyright
/////////////////////////////////////////////////////////////////////////////////////////////
// Copyright 2023 Garmin International, Inc.
// Licensed under the Flexible and Interoperable Data Transfer (FIT) Protocol License; you
// may not use this file except in compliance with the Flexible and Interoperable Data
// Transfer (FIT) Protocol License.
/////////////////////////////////////////////////////////////////////////////////////////////
// ****WARNING****  This file is auto-generated!  Do NOT edit this file.
// Profile Version = 21.105Release
// Tag = production/release/21.105.00-0-gdc65d24
/////////////////////////////////////////////////////////////////////////////////////////////

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
    /// Implements the DeveloperDataId profile message.
    /// </summary>
    public class DeveloperDataIdMesg : Mesg
    {
        #region Fields
        #endregion

        /// <summary>
        /// Field Numbers for <see cref="DeveloperDataIdMesg"/>
        /// </summary>
        public sealed class FieldDefNum
        {
            public const byte DeveloperId = 0;
            public const byte ApplicationId = 1;
            public const byte ManufacturerId = 2;
            public const byte DeveloperDataIndex = 3;
            public const byte ApplicationVersion = 4;
            public const byte Invalid = Fit.FieldNumInvalid;
        }

        #region Constructors
        public DeveloperDataIdMesg() : base(Profile.GetMesg(MesgNum.DeveloperDataId))
        {
        }

        public DeveloperDataIdMesg(Mesg mesg) : base(mesg)
        {
        }
        #endregion // Constructors

        #region Methods
        
        /// <summary>
        ///
        /// </summary>
        /// <returns>returns number of elements in field DeveloperId</returns>
        public int GetNumDeveloperId()
        {
            return GetNumFieldValues(0, Fit.SubfieldIndexMainField);
        }

        ///<summary>
        /// Retrieves the DeveloperId field</summary>
        /// <param name="index">0 based index of DeveloperId element to retrieve</param>
        /// <returns>Returns nullable byte representing the DeveloperId field</returns>
        public byte? GetDeveloperId(int index)
        {
            Object val = GetFieldValue(0, index, Fit.SubfieldIndexMainField);
            if(val == null)
            {
                return null;
            }

            return (Convert.ToByte(val));
            
        }

        /// <summary>
        /// Set DeveloperId field</summary>
        /// <param name="index">0 based index of developer_id</param>
        /// <param name="developerId_">Nullable field value to be set</param>
        public void SetDeveloperId(int index, byte? developerId_)
        {
            SetFieldValue(0, index, developerId_, Fit.SubfieldIndexMainField);
        }
        
        
        /// <summary>
        ///
        /// </summary>
        /// <returns>returns number of elements in field ApplicationId</returns>
        public int GetNumApplicationId()
        {
            return GetNumFieldValues(1, Fit.SubfieldIndexMainField);
        }

        ///<summary>
        /// Retrieves the ApplicationId field</summary>
        /// <param name="index">0 based index of ApplicationId element to retrieve</param>
        /// <returns>Returns nullable byte representing the ApplicationId field</returns>
        public byte? GetApplicationId(int index)
        {
            Object val = GetFieldValue(1, index, Fit.SubfieldIndexMainField);
            if(val == null)
            {
                return null;
            }

            return (Convert.ToByte(val));
            
        }

        /// <summary>
        /// Set ApplicationId field</summary>
        /// <param name="index">0 based index of application_id</param>
        /// <param name="applicationId_">Nullable field value to be set</param>
        public void SetApplicationId(int index, byte? applicationId_)
        {
            SetFieldValue(1, index, applicationId_, Fit.SubfieldIndexMainField);
        }
        
        ///<summary>
        /// Retrieves the ManufacturerId field</summary>
        /// <returns>Returns nullable ushort representing the ManufacturerId field</returns>
        public ushort? GetManufacturerId()
        {
            Object val = GetFieldValue(2, 0, Fit.SubfieldIndexMainField);
            if(val == null)
            {
                return null;
            }

            return (Convert.ToUInt16(val));
            
        }

        /// <summary>
        /// Set ManufacturerId field</summary>
        /// <param name="manufacturerId_">Nullable field value to be set</param>
        public void SetManufacturerId(ushort? manufacturerId_)
        {
            SetFieldValue(2, 0, manufacturerId_, Fit.SubfieldIndexMainField);
        }
        
        ///<summary>
        /// Retrieves the DeveloperDataIndex field</summary>
        /// <returns>Returns nullable byte representing the DeveloperDataIndex field</returns>
        public byte? GetDeveloperDataIndex()
        {
            Object val = GetFieldValue(3, 0, Fit.SubfieldIndexMainField);
            if(val == null)
            {
                return null;
            }

            return (Convert.ToByte(val));
            
        }

        /// <summary>
        /// Set DeveloperDataIndex field</summary>
        /// <param name="developerDataIndex_">Nullable field value to be set</param>
        public void SetDeveloperDataIndex(byte? developerDataIndex_)
        {
            SetFieldValue(3, 0, developerDataIndex_, Fit.SubfieldIndexMainField);
        }
        
        ///<summary>
        /// Retrieves the ApplicationVersion field</summary>
        /// <returns>Returns nullable uint representing the ApplicationVersion field</returns>
        public uint? GetApplicationVersion()
        {
            Object val = GetFieldValue(4, 0, Fit.SubfieldIndexMainField);
            if(val == null)
            {
                return null;
            }

            return (Convert.ToUInt32(val));
            
        }

        /// <summary>
        /// Set ApplicationVersion field</summary>
        /// <param name="applicationVersion_">Nullable field value to be set</param>
        public void SetApplicationVersion(uint? applicationVersion_)
        {
            SetFieldValue(4, 0, applicationVersion_, Fit.SubfieldIndexMainField);
        }
        
        #endregion // Methods
    } // Class
} // namespace
