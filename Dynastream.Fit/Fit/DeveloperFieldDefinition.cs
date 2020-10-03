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

namespace Dynastream.Fit
{
    /// <summary>
    /// Represents a Developer Field Definition
    /// </summary>
    internal class DeveloperFieldDefinition
    {
        private readonly FieldDescriptionMesg m_descriptionMesg;
        private readonly DeveloperDataIdMesg m_developerIdMesg;

        /// <summary>
        /// Gets a boolean indicating if the Field Definition has associated meta
        /// data
        /// </summary>
        public bool IsDefined
        {
            get { return !ReferenceEquals(m_descriptionMesg, null); }
        }

        /// <summary>
        /// Gets the Field Number associated with the Developer Field
        /// </summary>
        public byte FieldNum { get; private set; }

        /// <summary>
        /// Gets the Number of bytes associated with the Developer Field
        /// </summary>
        public byte Size { get; private set; }

        /// <summary>
        /// Gets the developer index of the Developer Field
        /// </summary>
        public byte DeveloperDataIndex { get; private set; }

        /// <summary>
        /// Gets the current description message for the field
        /// </summary>
        internal FieldDescriptionMesg DescriptionMesg
        {
            get { return m_descriptionMesg; }
        }

        /// <summary>
        /// Gets the Associated Developer Id for the message
        /// </summary>
        internal DeveloperDataIdMesg DeveloperIdMesg
        {
            get { return m_developerIdMesg; }
        }

        public DeveloperFieldDefinition(byte fieldNum, byte size, byte developerDataIndex)
        {
            m_descriptionMesg = null;
            FieldNum = fieldNum;
            Size = size;
            DeveloperDataIndex = developerDataIndex;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="desc"></param>
        /// <param name="devId"></param>
        /// <param name="size"></param>
        /// <exception cref="InvalidOperationException">
        ///     If description parameter is invalid
        /// </exception>
        public DeveloperFieldDefinition(FieldDescriptionMesg desc, DeveloperDataIdMesg devId, byte size)
        {
            byte? fieldDefinitionNumber = desc.GetFieldDefinitionNumber();
            byte? developerDataIndex = desc.GetDeveloperDataIndex();
            if ((developerDataIndex != null) &&
                (fieldDefinitionNumber != null))
            {
                m_descriptionMesg = desc;
                m_developerIdMesg = devId;
                Size = size;
                FieldNum = (byte)fieldDefinitionNumber;
                DeveloperDataIndex = (byte)developerDataIndex;
            }
            else
            {
                throw new FitException("Description Message must have a valid developer data index and field definition number");
            }
        }
    }
}
