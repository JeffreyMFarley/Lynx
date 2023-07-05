using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Data;
using System.Linq;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Threading;
using Esoteric;
using Esoteric.BLL.Interfaces;
using Esoteric.DAL.Interface;
using Lynx.Models;
using Lynx.Interfaces;

namespace Lynx.CSharpExchange
{
    [Export]
    public class ImportCSharpAssemblyToDomain : IEtlProcess
    {
        #region IoC Properties
        [Import]
        protected TypeExtractor Extract { get; set; }

        [Import]
        protected TypeToDomainTransform Transform { get; set; }

        [Import]
        protected IActiveDomain ActiveDomain { get; set; }
        #endregion

        #region Public Properties
        public FileInfo SourceFile { get; set; }
        #endregion

        #region IEtlProcess Members
        public bool Initialize()
        {
            if (SourceFile == null)
                return false;

            Extract.Source = SourceFile;

            Transform.EntitySetRepository = ActiveDomain.Manager.EntitySets;
            Transform.LinkSetRepository = ActiveDomain.Manager.LinkSets;

            return true;
        }

        public void Execute(IProgressUI progress)
        {
            progress.SetMinAndMax(0, 2);

            foreach (var e in Extract.EnumerateEntities())
                Transform.Accept(e);

            progress.Increment();

            foreach (var l in Extract.EnumerateLinks())
                Transform.Accept(l);

            progress.Increment();

#if DamerauLevenshteinVariations
            List<string> distinctProperties = new List<string>();

            var propertySet = EntitySetRepository.Get("Properties");
            foreach (var property in propertySet)
            {
                if (property.Name.Count() > 7 && !distinctProperties.Contains(property.Name))
                    distinctProperties.Add(property.Name);
            }

            EntitySet findVariations = new EntitySet("Variations");
            EntitySetRepository.Add(findVariations);

            findVariations.AddColumn("Distance", typeof(int));
            findVariations.AddColumn("Match", typeof(string));

            DateTime start = DateTime.Now;

            var search = from bb in propertySet
                         where bb.Description.ToLower().Contains("basis")
                         select bb.Name;

            foreach (string prop in search.OrderByDescending(s => s.Length).ThenBy(s => s))
            {
                Console.WriteLine("{1:mm\\.ss\\.fffff} {0}", prop, DateTime.Now - start);

                // See how many times the property occurs
                var owners = (from p in propertySet
                              where p.Name == prop
                              select p.Description).ToList();

                var maxDist = prop.Length / 3;

                foreach (var test in distinctProperties)
                {
                    if (test != prop )
                    {
                        var testOwners = (from t in propertySet
                                            where t.Name == test
                                            select t.Description).Except(owners);

                        // This is not testing the same class
                        if (testOwners.Count() > 0)
                        {
                            int dlValue = prop.DamerauLevenshtein(test);
                            if (dlValue > 0 && dlValue < maxDist )
                            {
                                Entity variation = findVariations.NewRow() as Entity;
                                variation.Name = prop;
                                variation.SetField<int>("Distance", dlValue);
                                variation.SetField<string>("Match", test);

                                Console.WriteLine("{0} {1} {2}", prop, dlValue, test);

                                findVariations.Add(variation);
                            }
                        }
                    }
                }
            }
#endif
        }

        public void Finish()
        {
        }
        #endregion
    }
}
