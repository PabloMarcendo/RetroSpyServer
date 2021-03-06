﻿using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Xml;

namespace SOAPMiddleware.Reader
{
    public class SOAPObjectReader
    {
        Message _requestMsg;
        OperationDescription _operationDesciption;
        ParameterInfo[] _paramInfoArray;
        List<object> _argumentList;
        XmlReader _xmlReader;

        public SOAPObjectReader(Message requestMsg, OperationDescription operationDescription)
        {
            _requestMsg = requestMsg;
            _operationDesciption = operationDescription;
            _paramInfoArray = operationDescription.DispatchMethod.GetParameters();
            _argumentList = new List<object>();
        }

        public List<object> GetRequestArguments()
        {
            _xmlReader = _requestMsg.GetReaderAtBodyContents();
            // Deserialize request wrapper and object
            bool IsStartElement = _xmlReader.IsStartElement(
                _operationDesciption.Name, _operationDesciption.Contract.Namespace);

            if (!IsStartElement)
            {
                return null;
            }

            foreach (var param in _paramInfoArray)
            {
                GetObjectFromRequest(param);
            }

            return _argumentList;
        }


        private void GetObjectFromRequest(ParameterInfo param)
        {
            if (param.ParameterType.IsClass)
            {
                _argumentList.Add(
                    GetObjectForClass(param));
            }
            else
            {
                _argumentList.Add(
                    GetObjectForNonClass(param));
            }
        }
        /// <summary>
        /// This function execute means http content is success fullly readed in to xmlreader
        /// If data contract can not correctly serialization plz check your DataContract class
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        private object GetObjectForClass(ParameterInfo param)
        {
            DataContractSerializer serializer =
                new DataContractSerializer(
                    param.ParameterType,
                    _operationDesciption.Name,
                    _operationDesciption.Contract.Namespace);


            return serializer.ReadObject(_xmlReader, verifyObjectName: true);
        }

        private object GetObjectForNonClass(ParameterInfo param)
        {
            // Find the element for the operation's data
            _xmlReader.ReadStartElement(_operationDesciption.Name, _operationDesciption.Contract.Namespace);

            string paramName = param.GetCustomAttribute<MessageParameterAttribute>()?.Name ?? param.Name;

            //_xmlReader.MoveToStartElement(paramName, _operationDesciption.Contract.Namespace);

            if (!_xmlReader.IsStartElement(paramName, _operationDesciption.Contract.Namespace))
            {
                return null;
            }
            else
            {
                return GetObjectForClass(param);
            }

            //DataContractSerializer serializer =
            //      new DataContractSerializer(
            //          param.ParameterType,
            //          paramName,
            //          _operationDesciption.Contract.Namespace);

            //return serializer.ReadObject(_xmlReader, verifyObjectName: true);
        }
    }
}
