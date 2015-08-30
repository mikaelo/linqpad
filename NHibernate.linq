<Query Kind="Program">
  <Reference>&lt;RuntimeDirectory&gt;\System.Messaging.dll</Reference>
  <NuGetReference>FluentNHibernate</NuGetReference>
  <NuGetReference>NHibernate</NuGetReference>
  <Namespace>FluentNHibernate.Cfg</Namespace>
  <Namespace>FluentNHibernate.Cfg.Db</Namespace>
  <Namespace>NHibernate</Namespace>
  <Namespace>NHibernate.Cfg</Namespace>
  <Namespace>NHibernate.Cfg.MappingSchema</Namespace>
  <Namespace>NHibernate.Dialect</Namespace>
  <Namespace>NHibernate.Linq</Namespace>
  <Namespace>NHibernate.Mapping.ByCode</Namespace>
  <Namespace>NHibernate.Mapping.ByCode.Conformist</Namespace>
  <Namespace>NHibernate.SqlCommand</Namespace>
  <Namespace>NHibernate.Transform</Namespace>
</Query>

void Main()
{
	Configuration cfg = new Configuration().DataBaseIntegration(db=>
	{
		db.ConnectionString = "Data Source=(LocalDB)\\MSSQLLocalDB;Initial Catalog=TestDB;Integrated Security=True;Pooling=False";
		db.Dialect<MsSql2012Dialect>();
	});
	
	var mapper = new ModelMapper();
	mapper.AddMappings(Assembly.GetExecutingAssembly().GetExportedTypes());
	
	HbmMapping mapping = mapper.CompileMappingForAllExplicitlyAddedEntities();
	cfg.AddMapping(mapping);
	
	using(ISessionFactory factory = cfg.BuildSessionFactory())
	using(ISession session = factory.OpenSession())
	using(ITransaction tx = session.BeginTransaction())
	{
		//var rules = session.Query<FraudRule>().Fetch(x=>x.FraudList).ToList();
		/*var query = session.CreateSQLQuery("exec fraud_GetCommonRules :IsWhitelist")
		.AddScalar("Id",NHibernateUtil.Int32)
		.AddScalar("Key",NHibernateUtil.String)
		.AddScalar("Value",NHibernateUtil.String)
		.AddScalar("Operator",NHibernateUtil.String)
		.AddScalar("Reason",NHibernateUtil.String)
		//.AddEntity(typeof(FraudRule))
		.SetBoolean("IsWhitelist", true)
		.SetResultTransformer(new AliasToBeanResultTransformer(typeof(FraudRule)))
		.List<FraudRule>();*/
		
		var query = session.Query<FraudRule>().Fetch(x=>x.FraudList).Where(x=>x.FraudList.IsWhitelist == false && x.FraudList.TerminalCode == null);
		
		List<FraudRule> rules = query.ToList();
		rules.Dump();
		tx.Commit();
	}
}

public class FraudRule
{
	public virtual int Id {get; set;}
	public virtual FraudList FraudList {get; set;}
	public virtual string Key {get; set;}
	public virtual string Operator {get; set;}
	public virtual string Value {get; set;}
	public virtual string Reason {get; set;}
}

public class FraudList
{
	public virtual int Id {get; set;}
	public virtual string Name {get; set;}
	public virtual string Reason {get; set;}
	public virtual string TerminalCode {get; set;}
	public virtual bool IsWhitelist {get; set;}
}

public class FraudRuleMap : ClassMapping<FraudRule>
{
	public FraudRuleMap()
	{
		this.Table("FraudRules");
		this.Id(p=>p.Id);
		this.Property(p=>p.Key, map=>map.Column("[Key]"));
		this.Property(p=>p.Operator);
		this.Property(p=>p.Value);
		this.Property(p=>p.Reason);
		this.ManyToOne(p=>p.FraudList, map=>map.Column("FraudListId"));
	}
}

public class FraudListMap : ClassMapping<FraudList>
{
	public FraudListMap()
	{
		this.Table("FraudLists");
		this.Id(p=>p.Id);
		this.Property(p=>p.Name);
		this.Property(p=>p.Reason);
		this.Property(p=>p.TerminalCode);
		this.Property(p=>p.IsWhitelist);
	}
}

// Define other methods and classes here