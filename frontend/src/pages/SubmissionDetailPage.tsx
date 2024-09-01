import React from 'react';
import { useParams } from 'react-router-dom';

const SubmissionDetail: React.FC = () => {
  const { id } = useParams<{ id: string }>();

  return (
    <div>
      <h1>Submission Detail</h1>
      <p>Submission ID: {id}</p>
      {/* Add more details about the submission here */}
    </div>
  );
};

export default SubmissionDetail;