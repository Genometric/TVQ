import React from 'react';
import clsx from 'clsx';
import Layout from '@theme/Layout';
import Link from '@docusaurus/Link';
import useDocusaurusContext from '@docusaurus/useDocusaurusContext';
import useBaseUrl from '@docusaurus/useBaseUrl';
import styles from './styles.module.css';

const features = [
  {
	title: 'Scope',
	description: (
      <>
		Study the impact of disseminating software 
		via package management systems: Bioconda, 
		Bioconductor, BioTools, and ToolShed.
      </>
	)
  },
  {
	title: 'Scale',
	description: (
      <>
		Studying more than 23,000 software packages and 
		their more than 18,000 scholarly articles.
      </>
	)
  },
  {
    title: 'Impact',
    //imageUrl: 'img/undraw_docusaurus_mountain.svg',
    description: (
      <>
        Motivate scientists to invest in disseminating 
		their software via package management systems. 
		Dissemination of more software via package 
		management systems will lead to a more straightforward 
		composition of computational pipelines and less 
		redundancy in software packages.
      </>
    ),
  }
];

function Feature({imageUrl, title, description}) {
  const imgUrl = useBaseUrl(imageUrl);
  return (
    <div className={clsx('col col--4', styles.feature)}>
      {imgUrl && (
        <div className="text--center">
          <img className={styles.featureImage} src={imgUrl} alt={title} />
        </div>
      )}
      <h3>{title}</h3>
      <p>{description}</p>
    </div>
  );
}

// To include site tittle and subtitile on the banner,
// use the following tags after `<div className="container">
//
// <h1 className="hero__title">{siteConfig.title}</h1>
// <p className="hero__subtitle">{siteConfig.tagline}</p>
//

function Home() {
  const context = useDocusaurusContext();
  const {siteConfig = {}} = context;
  return (
    <Layout
      title= {'TVQ'}
      description="Documentation for the Tool Visibility Quantifier (TVQ) project.">
      <header className={clsx('hero hero--primary', styles.heroBanner)}>
        <div className="container">
          <img src="logo/logo_hero_banner.svg" alt="logo" height="40%" width="40%"/>
          <p className="hero__subtitle"></p>
          <div className={styles.buttons}>
            <Link
              className={clsx(
                'button button--outline button--secondary button--lg',
                styles.getStarted,
              )}
              to={useBaseUrl('docs/getting_started/quickstart')}>
              Quick Start
            </Link>
          </div>
        </div>
      </header>
      <main>
        {features && features.length > 0 && (
          <section className={styles.features}>
            <div className="container">
              <div className="row">
                {features.map((props, idx) => (
                  <Feature key={idx} {...props} />
                ))}
              </div>
            </div>
          </section>
        )}
      </main>
    </Layout>
  );
}

export default Home;
