/*************************************************************************
 * ULLINK CONFIDENTIAL INFORMATION
 * _______________________________
 *
 * All Rights Reserved.
 *
 * NOTICE: This file and its content are the property of Ullink. The
 * information included has been classified as Confidential and may
 * not be copied, modified, distributed, or otherwise disseminated, in
 * whole or part, without the express written permission of Ullink.
 ************************************************************************/
using com.ullink.oms.model;
using java.lang;

namespace Ullink.desk.uitests.odisys
{
    internal class RejectException : Exception
    {
        public RejectException(Rej rej) : base(rej.toString())
        {
        }
    }
}