<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet exclude-result-prefixes="a" version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:cims="http://iec.ch/TC57/1999/rdf-schema-extensions-19990926#" xmlns:dc="http://purl.org/dc/elements/1.1/" xmlns:a="http://langdale.com.au/2005/Message#" xmlns:owl="http://www.w3.org/2002/07/owl#" xmlns:rdf="http://www.w3.org/1999/02/22-rdf-syntax-ns#" xmlns:rdfs="http://www.w3.org/2000/01/rdf-schema#" xmlns:uml="http://iec.ch/TC57/NonStandard/UML#" xmlns:xsd="http://www.w3.org/2001/XMLSchema#">
    <xsl:output xmlns:xalan="http://xml.apache.org/xslt" method="xml" omit-xml-declaration="no" indent="yes" xalan:indent-amount="4" />
    <xsl:param name="copyright" />
	<xsl:param name="version"/>
	<xsl:param name="baseURI"/>
	<xsl:param name="ontologyURI"/>
	<xsl:param name="envelope">Profile</xsl:param>
	<!-- The following <xsl:text> element is our newline representation where needed. -->
    <xsl:param name="newline"><xsl:text>
</xsl:text></xsl:param>

	<xsl:template match="a:Catalog">
		<xsl:value-of select="$newline"/>
		<xsl:if test="$copyright and $copyright != ''">
			<xsl:comment><xsl:value-of select="$newline"/><xsl:value-of select="$copyright" disable-output-escaping="yes"/><xsl:value-of select="$newline"/></xsl:comment>	
		</xsl:if>
		<xsl:value-of select="$newline"/>
		<rdf:RDF>
			<xsl:attribute name="xml:base" select="$ontologyURI"/>
			<xsl:variable name="rdf" as="node()*">
				<xsl:apply-templates select=".//*"/>
			</xsl:variable>
			<xsl:perform-sort select="$rdf">
				<xsl:sort select="@rdf:about"/>
			</xsl:perform-sort>
		</rdf:RDF>
	</xsl:template>

	<!-- ============================================================================================================ -->
	<!-- START SECTION:  (Packages, ComplexType, Root, EnumeratedType, SimpleType, Complex, & CompoundType(s)         -->
	<!-- templates) i.e. All "top level" element in the XML profile definition file...                                -->
	<!-- ============================================================================================================ -->
	<xsl:template match="a:Package">
		<rdf:Description rdf:about="#{substring-after(@basePackage, '#')}">
			<xsl:for-each select="a:ParentPackage">
				<cims:belongsToCategory rdf:resource="#{substring-after(@basePackage, '#')}"/>
			</xsl:for-each>
			<xsl:call-template name="stereotypes"/>
			<rdf:type rdf:resource="http://iec.ch/TC57/1999/rdf-schema-extensions-19990926#ClassCategory"/>
			<xsl:if test="a:Comment|a:Note"><rdfs:comment rdf:datatype="http://www.w3.org/2001/XMLSchema#string"><xsl:apply-templates select="." mode="comments"/></rdfs:comment></xsl:if>
			<rdfs:label xml:lang="en"><xsl:value-of select="@name"/></rdfs:label>
		</rdf:Description>
	</xsl:template>
	
	<xsl:template match="a:Root|a:ComplexType|a:EnumeratedType|a:CompoundType">
		<rdf:Description rdf:about="#{substring-after(@baseClass, '#')}">
			<cims:belongsToCategory rdf:resource="#{substring-after(@packageURI, '#')}"/>
			<xsl:call-template name="stereotypes"/>
			<rdf:type rdf:resource="http://www.w3.org/2000/01/rdf-schema#Class"/>
			<xsl:if test="a:Comment|a:Note"><rdfs:comment rdf:datatype="http://www.w3.org/2001/XMLSchema#string"><xsl:apply-templates select="." mode="comments"/></rdfs:comment></xsl:if>
			<!-- For top level classes (i.e. a:Root, a:ComplexType, a:EnumeratedType) we obtain the name 
				 of the class for the rdfs:label using the @baseClass attribute. This is different than 
				 what appears later in this XSLT for attributes and associations where the rdfs:label is 
				 determined using the @name attribute -->
			<rdfs:label xml:lang="en"><xsl:value-of select="substring-after(@baseClass, '#')"/></rdfs:label>
			<xsl:if test="a:SuperType">
				<!-- The name of the superclass is determined using the @baseClass attribute 
					 (and not the a:SuperType/@name) -->
				<rdfs:subClassOf rdf:resource="#{substring-after(a:SuperType/@baseClass, '#')}"/>
			</xsl:if>
		</rdf:Description>
	</xsl:template>
	
	<xsl:template match="a:SimpleType">	
		<rdf:Description rdf:about="#{substring-after(@dataType, '#')}">
			<cims:dataType rdf:resource="http://www.w3.org/2001/XMLSchema#{@xstype}"/>
			<xsl:call-template name="stereotypes"/>
			<rdf:type rdf:resource="http://www.w3.org/2000/01/rdf-schema#Datatype"/>
			<xsl:if test="a:Comment|a:Note"><rdfs:comment rdf:datatype="http://www.w3.org/2001/XMLSchema#string"><xsl:apply-templates select="." mode="comments"/></rdfs:comment></xsl:if>
			<rdfs:label xml:lang="en"><xsl:value-of select="@name"/></rdfs:label>
		</rdf:Description>
  	</xsl:template>
	<!-- ============================================================================================================ -->
	<!-- END SECTION:  Package, Complex, ComplexType, Root, & CompoundType(s) TYPE DEFINITION templates               -->
	<!-- ============================================================================================================ -->	
	
	<!-- ============================================================================================================ -->
	<!-- START SECTION:  (Simple, Domain, Instance, Reference attribute & association templates)                      -->
	<!-- ============================================================================================================ -->
	
	<xsl:template match="a:Simple">
		<rdf:Description rdf:about="#{substring-after(@baseProperty, '#')}">
			<cims:dataType rdf:resource="{@dataType}"/>
			<cims:multiplicity rdf:resource="http://iec.ch/TC57/1999/rdf-schema-extensions-19990926#M:{if (@minOccurs = @maxOccurs) then @minOccurs else concat(@minOccurs, '..', replace(@maxOccurs, 'unbounded', 'n'))}"/>
			<!-- This conditional is a "safety check" in the event that a a:Stereotype of type #attribute doesn't already 
				 exist. In that case we ensure we add it. This since all a:Domain elements are attributes. -->
			<xsl:if test="not(a:Stereotype[contains(., '#attribute')])">
				<cims:stereotype rdf:resource="http://iec.ch/TC57/NonStandard/UML#attribute"/>
			</xsl:if>
			<xsl:call-template name="stereotypes"/>
			<rdf:type rdf:resource="http://www.w3.org/1999/02/22-rdf-syntax-ns#Property"/>
			<xsl:if test="a:Comment|a:Note"><rdfs:comment rdf:datatype="http://www.w3.org/2001/XMLSchema#string"><xsl:apply-templates select="." mode="comments"/></rdfs:comment></xsl:if>
			<rdfs:domain rdf:resource="#{substring-before(substring-after(@baseProperty, '#'), '.')}"/>
			<rdfs:label xml:lang="en"><xsl:value-of select="@name"/></rdfs:label>
		</rdf:Description>
  	</xsl:template>
  	
	<xsl:template match="a:Domain">
		<rdf:Description rdf:about="#{substring-after(@baseProperty, '#')}">
			<cims:dataType rdf:resource="#{substring-after(@dataType, '#')}"/>
			<cims:multiplicity rdf:resource="http://iec.ch/TC57/1999/rdf-schema-extensions-19990926#M:{if (@minOccurs = @maxOccurs) then @minOccurs else concat(@minOccurs, '..', replace(@maxOccurs, 'unbounded', 'n'))}"/>
			<!-- This conditional is a "safety check" in the event that a a:Stereotype of type #attribute doesn't already 
				 exist. In that case we ensure we add it. This since all a:Domain elements are attributes. -->
			<xsl:if test="not(a:Stereotype[contains(., '#attribute')])">
				<cims:stereotype rdf:resource="http://iec.ch/TC57/NonStandard/UML#attribute"/>
			</xsl:if>
			<xsl:call-template name="stereotypes"/>
			<rdf:type rdf:resource="http://www.w3.org/1999/02/22-rdf-syntax-ns#Property"/>
			<xsl:if test="a:Comment|a:Note"><rdfs:comment rdf:datatype="http://www.w3.org/2001/XMLSchema#string"><xsl:apply-templates select="." mode="comments"/></rdfs:comment></xsl:if>
			<rdfs:domain rdf:resource="#{substring-before(substring-after(@baseProperty, '#'), '.')}"/>
			<!-- Note: this is intentionally using the @name attribute to source the label for a 
				 domain attribute defined for the class. -->
			<rdfs:label xml:lang="en"><xsl:value-of select="@name"/></rdfs:label>
		</rdf:Description>
	</xsl:template>

	<!-- ======================================================================= -->
	<!-- Elements of type a:Instance are always associations and not attributes. -->
	<!-- ======================================================================= -->
	<xsl:template match="a:Instance">
		<!-- Create the association -->
		<rdf:Description rdf:about="#{substring-after(@baseProperty, '#')}">
			<cims:AssociationUsed>Yes</cims:AssociationUsed>
			<xsl:if test="@inverseBaseProperty">
				<cims:inverseRoleName rdf:resource="#{substring-after(@inverseBaseProperty, '#')}"/>
			</xsl:if>
			<cims:multiplicity rdf:resource="http://iec.ch/TC57/1999/rdf-schema-extensions-19990926#M:{if (@minOccurs = @maxOccurs) then @minOccurs else concat(@minOccurs, '..', replace(@maxOccurs, 'unbounded', 'n'))}"/>
			<xsl:call-template name="stereotypes"/>
			<rdf:type rdf:resource="http://www.w3.org/1999/02/22-rdf-syntax-ns#Property"/>
			<xsl:if test="a:Comment|a:Note"><rdfs:comment rdf:datatype="http://www.w3.org/2001/XMLSchema#string"><xsl:apply-templates select="." mode="comments"/></rdfs:comment></xsl:if>
			<rdfs:domain rdf:resource="#{substring-before(substring-after(@baseProperty, '#'), '.')}"/>
			<rdfs:label xml:lang="en"><xsl:value-of select="@name"/></rdfs:label>
			<rdfs:range rdf:resource="#{@type}"/>
		</rdf:Description>
	</xsl:template>
	
	<xsl:template match="a:Reference">
		<!-- Create the association -->
		<rdf:Description rdf:about="#{substring-after(@baseProperty, '#')}">
			<cims:AssociationUsed>Yes</cims:AssociationUsed>
			<xsl:if test="@inverseBaseProperty">
				<cims:inverseRoleName rdf:resource="#{substring-after(@inverseBaseProperty, '#')}"/>
			</xsl:if>
			<cims:multiplicity rdf:resource="http://iec.ch/TC57/1999/rdf-schema-extensions-19990926#M:{if (@minOccurs = @maxOccurs) then @minOccurs else concat(@minOccurs, '..', replace(@maxOccurs, 'unbounded', 'n'))}"/>
			<!-- When an @inverseBaseProperty is missing it is a clear indicator that this is not actually an association but
				 rather is an attribute that the end user has not pulled in the type for in CIMTool while doing constructing
				 the profile. When that happens it appears as an a:Reference element and therefore the below conditional is 
				 required.  
			-->
			<xsl:if test="not(@inverseBaseProperty) and not(a:Stereotype[contains(., '#attribute')])">
				<cims:stereotype rdf:resource="http://iec.ch/TC57/NonStandard/UML#attribute"/>
			</xsl:if>
			<xsl:call-template name="stereotypes"/>
			<rdf:type rdf:resource="http://www.w3.org/1999/02/22-rdf-syntax-ns#Property"/>
			<xsl:if test="a:Comment|a:Note"><rdfs:comment rdf:parseType="Literal"><xsl:apply-templates select="." mode="comments"/></rdfs:comment></xsl:if>
			<rdfs:domain rdf:resource="#{substring-before(substring-after(@baseProperty, '#'), '.')}"/>
			<rdfs:label xml:lang="en"><xsl:value-of select="@name"/></rdfs:label>
			<rdfs:range rdf:resource="#{@type}"/>
		</rdf:Description>
	</xsl:template>
	
	<xsl:template match="a:Enumerated">	
		<rdf:Description rdf:about="#{substring-after(@baseProperty, '#')}">
			<cims:multiplicity rdf:resource="http://iec.ch/TC57/1999/rdf-schema-extensions-19990926#M:{if (@minOccurs = @maxOccurs) then @minOccurs else concat(@minOccurs, '..', replace(@maxOccurs, 'unbounded', 'n'))}"/>
			<xsl:if test="not(a:Stereotype[contains(., '#attribute')])">
				<cims:stereotype rdf:resource="http://iec.ch/TC57/NonStandard/UML#attribute"/>
			</xsl:if>
			<xsl:call-template name="stereotypes"/>	
			<rdf:type rdf:resource="http://www.w3.org/1999/02/22-rdf-syntax-ns#Property"/>
			<xsl:if test="a:Comment|a:Note"><rdfs:comment rdf:datatype="http://www.w3.org/2001/XMLSchema#string"><xsl:apply-templates select="." mode="comments"/></rdfs:comment></xsl:if>
			<rdfs:domain rdf:resource="#{substring-before(substring-after(@baseProperty, '#'), '.')}"/>
			<!-- Note: this is intentionally using the @name attribute to source the label for an 
				 enumerated attribute within a class. -->
			<rdfs:label xml:lang="en"><xsl:value-of select="@name"/></rdfs:label>
			<rdfs:range rdf:resource="#{@type}"/>
		</rdf:Description>
	</xsl:template>
	
	<xsl:template match="a:EnumeratedValue">	
		<rdf:Description rdf:about="#{substring-after(@baseResource, '#')}">
			<xsl:if test="not(a:Stereotype[contains(., '#enum')])">
				<cims:stereotype>enum</cims:stereotype>
			</xsl:if>
			<xsl:call-template name="stereotypes"/>	
			<rdf:type rdf:resource="#{substring-before(substring-after(@baseResource, '#'), '.')}"/>
			<xsl:if test="a:Comment|a:Note"><rdfs:comment rdf:datatype="http://www.w3.org/2001/XMLSchema#string"><xsl:apply-templates select="." mode="comments"/></rdfs:comment></xsl:if>
			<rdfs:label xml:lang="en"><xsl:value-of select="@name"/></rdfs:label>
		</rdf:Description>
	</xsl:template>
	<!-- ============================================================================================================ -->
	<!-- END SECTION:  (ComplexType, Root, and EnumeratedType attribute & association templates)                    -->
	<!-- ============================================================================================================ -->

<!--
	<xsl:template match="a:Complex">
		<xs:element name="{@name}" minOccurs="{@minOccurs}" maxOccurs="{@maxOccurs}" sawsdl:modelReference="{@baseProperty}">
			<xsl:call-template name="annotate"/>
			<xs:complexType sawsdl:modelReference="{@baseClass}">
				<xsl:call-template name="type_body"/>
			</xs:complexType>
		</xs:element>
	</xsl:template>
	
	<xsl:template match="a:Choice[a:Stereotype='http://langdale.com.au/2005/UML#preserve']">
		<xs:element name="{@name}" minOccurs="{@minOccurs}" maxOccurs="{@maxOccurs}" sawsdl:modelReference="{@baseProperty}">
			<xsl:call-template name="annotate"/>
			<xs:complexType>
				<xs:choice>
					<xsl:apply-templates/>
				</xs:choice>
			</xs:complexType>
		</xs:element>
	</xsl:template>
	<xsl:template match="a:SimpleEnumerated">
		<xs:element name="{@name}" minOccurs="{@minOccurs}" maxOccurs="{@maxOccurs}" sawsdl:modelReference="{@baseProperty}">
			<xsl:call-template name="annotate"/>
			<xs:simpleType sawsdl:modelReference="{@baseClass}">
				<xs:restriction base="xs:string">
					<xsl:apply-templates/>
				</xs:restriction>
			</xs:simpleType>
		</xs:element>
	</xsl:template>
	<xsl:template match="a:CompoundType" mode="declare">
		<xs:complexType name="{@name}" sawsdl:modelReference="{@baseClass}">
			<xsl:call-template name="annotate"/>
			<xsl:call-template name="type_body"/>
		</xs:complexType>
	</xsl:template>
	-->
	<xsl:template name="stereotypes">
		<xsl:for-each select="a:Stereotype">
			<xsl:sort select="." order="ascending" />
			<xsl:variable name="stereotype" select="substring-after(., '#')"/>
			<xsl:choose>
				<xsl:when test="($stereotype = 'attribute') or ($stereotype = 'byreference') or ($stereotype = 'enumeration') or ($stereotype = 'enum')or ($stereotype = 'concrete') or ($stereotype = 'ofAggregate') or ($stereotype = 'aggregateOf') or ($stereotype = 'ofComposite') or ($stereotype = 'compositeOf') or ($stereotype = 'compound')">
					<cims:stereotype rdf:resource="http://iec.ch/TC57/NonStandard/UML#{$stereotype}"/>
				</xsl:when>
				<xsl:otherwise>
					<cims:stereotype><xsl:value-of select="@label"/></cims:stereotype>
				</xsl:otherwise>
			</xsl:choose>
		</xsl:for-each>
	</xsl:template>
	<xsl:template match="." mode="comments">
		<xsl:for-each select="a:Comment|a:Note">
			<xsl:value-of select="."/><xsl:if test="position()!=last()"><xsl:value-of select="$newline"/></xsl:if>
		</xsl:for-each>
	</xsl:template>
	<xsl:template match="text()">
		<!-- dont pass text through -->
	</xsl:template>
</xsl:stylesheet>
